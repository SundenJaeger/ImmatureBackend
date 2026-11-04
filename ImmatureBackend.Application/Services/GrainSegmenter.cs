using SkiaSharp;

namespace ImmatureBackend.Application.Services;

public sealed record GrainSegment(int X, int Y, int Width, int Height, bool[] Mask);

public static class GrainSegmenter
{
    private const int BorderWidth = 4;
    private const int MinThreshold = 20;
    private const int MinGrainArea = 100;

    private const double MinRelativeArea = 0.3;
    private const double MaxRelativeArea = 3.0;

    public static List<GrainSegment> Segment(SKColor[] pixels, int width, int height)
    {
        ArgumentNullException.ThrowIfNull(pixels);

        if (pixels.Length != checked(width * height))
        {
            throw new ArgumentException("Pixel array dimensions do not match.", nameof(pixels));
        }

        var distance = DistanceFromBackground(pixels, width, height);
        var threshold = Math.Max(Otsu(distance), MinThreshold);
        var components = LabelComponents(distance, threshold, width, height, out var labels);

        var candidates = new List<Component>(components.Count);

        foreach (var component in components)
        {
            if (component.Area >= MinGrainArea)
            {
                candidates.Add(component);
            }
        }

        if (candidates.Count == 0)
        {
            return [];
        }

        var areas = new int[candidates.Count];

        for (var i = 0; i < candidates.Count; i++)
        {
            areas[i] = candidates[i].Area;
        }

        Array.Sort(areas);

        var median = areas[areas.Length / 2];

        var minimumArea = median * MinRelativeArea;
        var maximumArea = median * MaxRelativeArea;

        var result = new List<GrainSegment>(candidates.Count);

        foreach (var component in candidates)
        {
            if (component.Area < minimumArea ||
                component.Area > maximumArea)
            {
                continue;
            }

            result.Add(ToSegment(component, labels, width));
        }

        return result;
    }

    private static byte[] DistanceFromBackground(SKColor[] pixels, int width, int height)
    {
        Span<int> redHistogram = stackalloc int[256];
        Span<int> greenHistogram = stackalloc int[256];
        Span<int> blueHistogram = stackalloc int[256];

        var borderPixelCount = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var isBorder =
                    x < BorderWidth ||
                    y < BorderWidth ||
                    x >= width - BorderWidth ||
                    y >= height - BorderWidth;

                if (!isBorder)
                {
                    continue;
                }

                var pixel = pixels[y * width + x];

                redHistogram[pixel.Red]++;
                greenHistogram[pixel.Green]++;
                blueHistogram[pixel.Blue]++;

                borderPixelCount++;
            }
        }

        var backgroundRed = MedianFromHistogram(redHistogram, borderPixelCount);
        var backgroundGreen = MedianFromHistogram(greenHistogram, borderPixelCount);
        var backgroundBlue = MedianFromHistogram(blueHistogram, borderPixelCount);

        var distance = new byte[pixels.Length];

        for (var i = 0; i < pixels.Length; i++)
        {
            var pixel = pixels[i];

            var difference =
                Math.Abs(pixel.Red - backgroundRed) +
                Math.Abs(pixel.Green - backgroundGreen) +
                Math.Abs(pixel.Blue - backgroundBlue);

            distance[i] = (byte)(difference / 3);
        }

        return distance;
    }

    private static int MedianFromHistogram(
        ReadOnlySpan<int> histogram,
        int count)
    {
        var target = count / 2;
        var cumulative = 0;

        for (var i = 0; i < histogram.Length; i++)
        {
            cumulative += histogram[i];

            if (cumulative > target)
            {
                return i;
            }
        }

        return 0;
    }

    private static int Otsu(byte[] values)
    {
        var histogram = new long[256];

        foreach (var value in values)
        {
            histogram[value]++;
        }

        double total = values.Length;
        double sumAll = 0;

        for (var i = 0; i < 256; i++)
        {
            sumAll += i * (double)histogram[i];
        }

        double sumBackground = 0;
        double weightBackground = 0;
        double bestVariance = -1;

        var best = 0;

        for (var t = 0; t < 256; t++)
        {
            weightBackground += histogram[t];

            if (weightBackground == 0)
            {
                continue;
            }

            var weightForeground =
                total - weightBackground;

            if (weightForeground == 0)
            {
                break;
            }

            sumBackground += t * (double)histogram[t];

            var meanBackground = sumBackground / weightBackground;
            var meanForeground = (sumAll - sumBackground) / weightForeground;
            var difference = meanBackground - meanForeground;

            var variance =
                weightBackground *
                weightForeground *
                difference *
                difference;

            if (variance > bestVariance)
            {
                bestVariance = variance;
                best = t;
            }
        }

        return best;
    }

    private readonly record struct Component(int Id, int Area, int MinX, int MinY, int MaxX, int MaxY);

    private static List<Component> LabelComponents(byte[] distance, int threshold, int width, int height,
        out int[] labels)
    {
        var lab = new int[distance.Length];
        var components = new List<Component>();
        var stack = new Stack<int>();

        var id = 0;

        for (var start = 0; start < distance.Length; start++)
        {
            if (distance[start] <= threshold || lab[start] != 0)
            {
                continue;
            }

            id++;

            lab[start] = id;
            stack.Push(start);

            var area = 0;

            var minX = width;
            var minY = height;
            var maxX = 0;
            var maxY = 0;

            while (stack.Count > 0)
            {
                var position = stack.Pop();

                var x = position % width;
                var y = position / width;

                area++;

                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);

                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);

                for (var k = 0; k < 4; k++)
                {
                    var neighbor = k switch
                    {
                        0 when x > 0 => position - 1,
                        1 when x < width - 1 => position + 1,
                        2 when y > 0 => position - width,
                        3 when y < height - 1 => position + width,
                        _ => -1
                    };

                    if (neighbor < 0 ||
                        distance[neighbor] <= threshold ||
                        lab[neighbor] != 0)
                    {
                        continue;
                    }

                    lab[neighbor] = id;
                    stack.Push(neighbor);
                }
            }

            components.Add(new Component(id, area, minX, minY, maxX, maxY));
        }

        labels = lab;

        return components;
    }

    private static GrainSegment ToSegment(Component component, int[] labels, int imageWidth)
    {
        var width = component.MaxX - component.MinX + 1;
        var height = component.MaxY - component.MinY + 1;
        var mask = new bool[width * height];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = (component.MinY + y) * imageWidth + component.MinX;
            var maskRow = y * width;

            for (var x = 0; x < width; x++)
            {
                mask[maskRow + x] = labels[sourceRow + x] == component.Id;
            }
        }

        return new GrainSegment(
            component.MinX,
            component.MinY,
            width,
            height,
            mask
        );
    }
}