using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace ImmatureBackend.Application.Services;

public sealed class OnnxGrainDetector : IGrainDetector, IDisposable
{
    private const double ConfidenceThreshold = 0.5;
    private const int ImmatureClass = 0;
    private const int InputSize = 160;
    private const int BatchSize = 16;
    private const int Channels = 3;
    private const int TensorImageSize = Channels * InputSize * InputSize;

    private static readonly float[] Mean =
    [
        0.485f, 0.456f, 0.406f
    ];

    private static readonly float[] Std =
    [
        0.229f, 0.224f, 0.225f
    ];

    private readonly InferenceSession _session;
    private readonly ILogger<OnnxGrainDetector> _logger;

    private readonly float[] _inputBuffer = new float[BatchSize * TensorImageSize];

    private readonly Lock _requestLock = new();

    private bool _disposed;

    public OnnxGrainDetector(string modelPath, ILogger<OnnxGrainDetector> logger)
    {
        _logger = logger;

        using var options = new SessionOptions
        {
            IntraOpNumThreads = Math.Min(4, Environment.ProcessorCount),
            InterOpNumThreads = 1,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        _session = new InferenceSession(modelPath, options);

        _logger.LogInformation("Grain classifier loaded from {ModelPath}.", modelPath);
    }

    public PredictResponse Detect(byte[] imageBytes)
    {
        lock (_requestLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return DetectCore(imageBytes);
        }
    }

    private PredictResponse DetectCore(byte[] imageBytes)
    {
        using var bitmap = SKBitmap.Decode(imageBytes)
                           ?? throw new InvalidOperationException("Image could not be decoded.");

        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixels = bitmap.Pixels;

        var segments = GrainSegmenter.Segment(pixels, width, height);
        var immatureProbability = Classify(pixels, width, segments);

        var grains = new List<GrainBox>();

        for (var i = 0; i < segments.Count; i++)
        {
            var confidence = immatureProbability[i];

            if (confidence < ConfidenceThreshold)
            {
                continue;
            }

            var segment = segments[i];

            grains.Add(new GrainBox
            {
                X = segment.X,
                Y = segment.Y,
                Width = segment.Width,
                Height = segment.Height,
                Confidence = Math.Round(confidence, 2),
                Action = null
            });
        }

        _logger.LogInformation(
            "Grain detection: {Segmented} grains found, {Immature} classified immature. Image: {Width}x{Height}.",
            segments.Count,
            grains.Count,
            width,
            height);

        return new PredictResponse
        {
            ImageId = Guid.NewGuid().ToString(),
            Grains = grains
        };
    }

    private double[] Classify(SKColor[] pixels, int imageWidth, List<GrainSegment> segments)
    {
        var result = new double[segments.Count];

        for (var offset = 0; offset < segments.Count; offset += BatchSize)
        {
            var count = Math.Min(BatchSize, segments.Count - offset);

            var tensorLength = count * TensorImageSize;

            var dimensions = new[]
            {
                count,
                Channels,
                InputSize,
                InputSize
            };

            var input = new DenseTensor<float>(_inputBuffer.AsMemory(0, tensorLength), dimensions);

            for (var i = 0; i < count; i++)
            {
                FillTensor(input, i, pixels, imageWidth, segments[offset + i]);
            }

            var inputs = new[]
            {
                NamedOnnxValue.CreateFromTensor("image", input)
            };

            using var outputs = _session.Run(inputs);

            var probabilities = outputs[0].AsTensor<float>();

            for (var i = 0; i < count; i++)
            {
                result[offset + i] = probabilities[i, ImmatureClass];
            }
        }

        return result;
    }

    private static void FillTensor(DenseTensor<float> tensor, int index, SKColor[] pixels, int imageWidth,
        GrainSegment segment)
    {
        var side = Math.Max(segment.Width, segment.Height);

        var offsetX = (side - segment.Width) / 2;
        var offsetY = (side - segment.Height) / 2;

        var info = new SKImageInfo(
            side,
            side,
            SKColorType.Rgba8888,
            SKAlphaType.Opaque
        );

        using var squareBitmap = new SKBitmap(info);

        squareBitmap.Erase(SKColors.Black);

        for (var y = 0; y < segment.Height; y++)
        {
            var sourceRow = (segment.Y + y) * imageWidth;
            var maskRow = y * segment.Width;
            var destinationY = offsetY + y;

            for (var x = 0; x < segment.Width; x++)
            {
                if (!segment.Mask[maskRow + x])
                {
                    continue;
                }

                var sourcePixel = pixels[sourceRow + segment.X + x];

                squareBitmap.SetPixel(offsetX + x, destinationY, sourcePixel);
            }
        }

        using var resized = squareBitmap.Resize(
                                new SKImageInfo(InputSize, InputSize, SKColorType.Rgba8888, SKAlphaType.Opaque),
                                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
                            ?? throw new InvalidOperationException("Grain crop could not be resized.");

        var resizedPixels = resized.Pixels;
        
        for (var y = 0; y < InputSize; y++)
        {
            var rowOffset = y * InputSize;

            for (var x = 0; x < InputSize; x++)
            {
                var pixel = resizedPixels[rowOffset + x];
                
                tensor[index, 0, y, x] = (pixel.Red / 255f - Mean[0]) / Std[0];
                tensor[index, 1, y, x] = (pixel.Green / 255f - Mean[1]) / Std[1];
                tensor[index, 2, y, x] = (pixel.Blue / 255f - Mean[2]) / Std[2];
            }
        }
    }

    public void Dispose()
    {
        lock (_requestLock)
        {
            if (_disposed)
            {
                return;
            }

            _session.Dispose();
            _disposed = true;
        }
    }
}