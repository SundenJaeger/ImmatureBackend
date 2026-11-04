namespace ImmatureBackend.Application.Services;

public sealed record GrainSegment(int X, int Y, int Width, int Height, bool[] Mask);

public static class GrainSegmenter
{
    private const int BorderWidth = 4;
    private const int MinThreshold = 20;
    private const int MinGrainArea = 100;
    private const double MinRelativeArea = 0.3;
    private const double MaxRelativeArea = 3.0;
    
    public static List<GrainSegment> Segment(byte[] rgb, int width, int height)
    {
        var distance = DistanceFromBackground(rgb, width, height);
        var threshold = Math.Max(Otsu(distance), MinThreshold);

        var foreground = new bool[distance.Length];
        for (var i = 0; i < distance.Length; i++)
        {
            foreground[i] = distance[i] > threshold;
        }

        var components = LabelComponents(foreground, width, height, out var labels);

        var candidates = components.Where(c => c.Area >= MinGrainArea).ToList();
        if (candidates.Count == 0)
        {
            return [];
        }

        var median = Median(candidates.Select(c => c.Area).ToList());
        var kept = candidates.Where(c => c.Area >= median * MinRelativeArea && c.Area <= median * MaxRelativeArea);

        return kept.Select(c => ToSegment(c, labels, width)).ToList();
    }
    
    private static byte[] DistanceFromBackground(byte[] rgb, int width, int height)
    {
        var r = new List<byte>();
        var g = new List<byte>();
        var b = new List<byte>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (x >= BorderWidth && y >= BorderWidth && x < width - BorderWidth && y < height - BorderWidth)
                {
                    x = width - BorderWidth - 1;
                    continue;
                }

                var o = (y * width + x) * 3;
                r.Add(rgb[o]);
                g.Add(rgb[o + 1]);
                b.Add(rgb[o + 2]);
            }
        }

        var bg = (R: MedianByte(r), G: MedianByte(g), B: MedianByte(b));

        var result = new byte[width * height];
        for (var i = 0; i < result.Length; i++)
        {
            var o = i * 3;
            var d = (Math.Abs(rgb[o] - bg.R) + Math.Abs(rgb[o + 1] - bg.G) + Math.Abs(rgb[o + 2] - bg.B)) / 3;
            result[i] = (byte)d;
        }

        return result;
    }

    private static int Otsu(byte[] values)
    {
        var histogram = new long[256];
        foreach (var v in values)
        {
            histogram[v]++;
        }

        double total = values.Length;
        double sumAll = 0;
        for (var i = 0; i < 256; i++)
        {
            sumAll += i * (double)histogram[i];
        }

        double sumBackground = 0, weightBackground = 0, bestVariance = -1;
        var best = 0;

        for (var t = 0; t < 256; t++)
        {
            weightBackground += histogram[t];
            if (weightBackground == 0)
            {
                continue;
            }

            var weightForeground = total - weightBackground;
            if (weightForeground == 0)
            {
                break;
            }

            sumBackground += t * (double)histogram[t];
            var meanBackground = sumBackground / weightBackground;
            var meanForeground = (sumAll - sumBackground) / weightForeground;
            var variance = weightBackground * weightForeground * Math.Pow(meanBackground - meanForeground, 2);

            if (variance > bestVariance)
            {
                bestVariance = variance;
                best = t;
            }
        }

        return best;
    }

    private readonly record struct Component(int Id, int Area, int MinX, int MinY, int MaxX, int MaxY);
    
    private static List<Component> LabelComponents(bool[] foreground, int width, int height, out int[] labels)
    {
        var lab = new int[foreground.Length];
        var components = new List<Component>();
        var stack = new Stack<int>();
        var id = 0;

        for (var start = 0; start < foreground.Length; start++)
        {
            if (!foreground[start] || lab[start] != 0)
            {
                continue;
            }

            id++;
            lab[start] = id;
            stack.Push(start);

            int area = 0, minX = width, minY = height, maxX = 0, maxY = 0;

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                int x = p % width, y = p / width;

                area++;
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);

                for (var k = 0; k < 4; k++)
                {
                    var n = k switch
                    {
                        0 when x > 0 => p - 1,
                        1 when x < width - 1 => p + 1,
                        2 when y > 0 => p - width,
                        3 when y < height - 1 => p + width,
                        _ => -1
                    };

                    if (n >= 0 && foreground[n] && lab[n] == 0)
                    {
                        lab[n] = id;
                        stack.Push(n);
                    }
                }
            }

            components.Add(new Component(id, area, minX, minY, maxX, maxY));
        }

        labels = lab;
        return components;
    }

    private static GrainSegment ToSegment(Component c, int[] labels, int imageWidth)
    {
        var w = c.MaxX - c.MinX + 1;
        var h = c.MaxY - c.MinY + 1;
        var mask = new bool[w * h];

        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
            {
                mask[y * w + x] = labels[(c.MinY + y) * imageWidth + c.MinX + x] == c.Id;
            }
        }

        return new GrainSegment(c.MinX, c.MinY, w, h, mask);
    }

    private static int Median(List<int> values)
    {
        values.Sort();
        return values[values.Count / 2];
    }

    private static byte MedianByte(List<byte> values)
    {
        values.Sort();
        return values[values.Count / 2];
    }
}