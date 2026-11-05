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
    private const int BatchSize = 4;
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
            IntraOpNumThreads = 1,
            InterOpNumThreads = 1,
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            EnableCpuMemArena = false,
            EnableMemoryPattern = false
        };
        options.AddSessionConfigEntry("session.intra_op.allow_spinning", "0");

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
                FillTensor(
                    _inputBuffer.AsSpan(i * TensorImageSize, TensorImageSize),
                    pixels,
                    imageWidth,
                    segments[offset + i]);
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
    
    private static void FillTensor(Span<float> destination, SKColor[] pixels, int imageWidth, GrainSegment segment)
    {
        const int plane = InputSize * InputSize;

        var side = Math.Max(segment.Width, segment.Height);
        var offsetX = (side - segment.Width) / 2;
        var offsetY = (side - segment.Height) / 2;
        var scale = side / (float)InputSize;
        var last = side - 1;

        for (var y = 0; y < InputSize; y++)
        {
            var sourceY = Math.Clamp((y + 0.5f) * scale - 0.5f, 0f, last);
            var y0 = (int)sourceY;
            var y1 = Math.Min(y0 + 1, last);
            var fy = sourceY - y0;

            for (var x = 0; x < InputSize; x++)
            {
                var sourceX = Math.Clamp((x + 0.5f) * scale - 0.5f, 0f, last);
                var x0 = (int)sourceX;
                var x1 = Math.Min(x0 + 1, last);
                var fx = sourceX - x0;

                var p00 = MaskedPixel(pixels, imageWidth, segment, offsetX, offsetY, x0, y0);
                var p10 = MaskedPixel(pixels, imageWidth, segment, offsetX, offsetY, x1, y0);
                var p01 = MaskedPixel(pixels, imageWidth, segment, offsetX, offsetY, x0, y1);
                var p11 = MaskedPixel(pixels, imageWidth, segment, offsetX, offsetY, x1, y1);

                var w00 = (1 - fx) * (1 - fy);
                var w10 = fx * (1 - fy);
                var w01 = (1 - fx) * fy;
                var w11 = fx * fy;

                var red = p00.Red * w00 + p10.Red * w10 + p01.Red * w01 + p11.Red * w11;
                var green = p00.Green * w00 + p10.Green * w10 + p01.Green * w01 + p11.Green * w11;
                var blue = p00.Blue * w00 + p10.Blue * w10 + p01.Blue * w01 + p11.Blue * w11;

                var index = y * InputSize + x;
                destination[index] = (red / 255f - Mean[0]) / Std[0];
                destination[plane + index] = (green / 255f - Mean[1]) / Std[1];
                destination[2 * plane + index] = (blue / 255f - Mean[2]) / Std[2];
            }
        }
    }
    
    private static SKColor MaskedPixel(SKColor[] pixels, int imageWidth, GrainSegment segment,
        int offsetX, int offsetY, int x, int y)
    {
        var gx = x - offsetX;
        var gy = y - offsetY;

        if (gx < 0 || gx >= segment.Width || gy < 0 || gy >= segment.Height ||
            !segment.Mask[gy * segment.Width + gx])
        {
            return default;
        }

        return pixels[(segment.Y + gy) * imageWidth + segment.X + gx];
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