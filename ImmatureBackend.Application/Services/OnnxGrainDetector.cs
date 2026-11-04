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
    private const int BatchSize = 64;

    private static readonly float[] Mean = [0.485f, 0.456f, 0.406f];
    private static readonly float[] Std = [0.229f, 0.224f, 0.225f];

    private readonly InferenceSession _session;
    private readonly ILogger<OnnxGrainDetector> _logger;

    public OnnxGrainDetector(string modelPath, ILogger<OnnxGrainDetector> logger)
    {
        _logger = logger;
        _session = new InferenceSession(modelPath);
        _logger.LogInformation("Grain classifier loaded from {ModelPath}.", modelPath);
    }

    public PredictResponse Detect(byte[] imageBytes)
    {
        using var bitmap = SKBitmap.Decode(imageBytes)
                           ?? throw new InvalidOperationException("Image could not be decoded.");
        var width = bitmap.Width;
        var pixels = bitmap.Pixels;

        var rgb = new byte[width * bitmap.Height * 3];
        for (var i = 0; i < pixels.Length; i++)
        {
            rgb[i * 3] = pixels[i].Red;
            rgb[i * 3 + 1] = pixels[i].Green;
            rgb[i * 3 + 2] = pixels[i].Blue;
        }

        var segments = GrainSegmenter.Segment(rgb, width, bitmap.Height);
        var immatureProbability = Classify(pixels, width, segments);

        var grains = new List<GrainBox>();
        for (var i = 0; i < segments.Count; i++)
        {
            if (immatureProbability[i] < ConfidenceThreshold)
            {
                continue;
            }

            grains.Add(new GrainBox
            {
                X = segments[i].X,
                Y = segments[i].Y,
                Width = segments[i].Width,
                Height = segments[i].Height,
                Confidence = Math.Round(immatureProbability[i], 2),
                Action = null
            });
        }

        _logger.LogInformation(
            "Grain detection: {Segmented} grains found, {Immature} classified immature.",
            segments.Count,
            grains.Count);

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
            var input = new DenseTensor<float>(new[] { count, 3, InputSize, InputSize });

            for (var i = 0; i < count; i++)
            {
                FillTensor(input, i, pixels, imageWidth, segments[offset + i]);
            }

            using var outputs = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("image", input) });
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

        var square = new SKColor[side * side];
        Array.Fill(square, SKColors.Black);
        for (var y = 0; y < segment.Height; y++)
        {
            for (var x = 0; x < segment.Width; x++)
            {
                if (segment.Mask[y * segment.Width + x])
                {
                    square[(offsetY + y) * side + offsetX + x] = pixels[(segment.Y + y) * imageWidth + segment.X + x];
                }
            }
        }

        var info = new SKImageInfo(side, side, SKColorType.Rgba8888, SKAlphaType.Opaque);
        using var squareBitmap = new SKBitmap(info);
        squareBitmap.Pixels = square;
        using var resized = squareBitmap.Resize(
                                new SKImageInfo(InputSize, InputSize, SKColorType.Rgba8888, SKAlphaType.Opaque),
                                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
                            ?? throw new InvalidOperationException("Grain crop could not be resized.");

        var resizedPixels = resized.Pixels;
        for (var y = 0; y < InputSize; y++)
        {
            for (var x = 0; x < InputSize; x++)
            {
                var p = resizedPixels[y * InputSize + x];
                tensor[index, 0, y, x] = (p.Red / 255f - Mean[0]) / Std[0];
                tensor[index, 1, y, x] = (p.Green / 255f - Mean[1]) / Std[1];
                tensor[index, 2, y, x] = (p.Blue / 255f - Mean[2]) / Std[2];
            }
        }
    }

    public void Dispose() => _session.Dispose();
}