using ImmatureBackend.Application.Services;
using SkiaSharp;

namespace ImmatureBackend.Test.Application.Services;

public class GrainSegmenterTests
{
    private const int Size = 200;

    private static SKColor[] Canvas((byte R, byte G, byte B) background)
    {
        var color = new SKColor(background.R, background.G, background.B);

        return Enumerable.Repeat(color, Size * Size).ToArray();
    }

    private static void Rect(SKColor[] pixels, int x, int y, int w, int h, (byte R, byte G, byte B) color)
    {
        var skColor = new SKColor(color.R, color.G, color.B);

        for (var j = y; j < y + h; j++)
        {
            for (var i = x; i < x + w; i++)
            {
                pixels[j * Size + i] = skColor;
            }
        }
    }

    [Fact]
    public void Segment_FindsEachSeparateGrainWithItsBox()
    {
        var pixels = Canvas((10, 10, 10));
        Rect(pixels, 20, 30, 20, 20, (200, 200, 190));
        Rect(pixels, 100, 60, 22, 18, (120, 180, 90));
        Rect(pixels, 150, 150, 20, 20, (220, 220, 220));

        var segments = GrainSegmenter.Segment(pixels, Size, Size);

        Assert.Equal(3, segments.Count);
        Assert.Contains(segments, s => s is { X: 20, Y: 30, Width: 20, Height: 20 });
        Assert.Contains(segments, s => s is { X: 100, Y: 60, Width: 22, Height: 18 });
        Assert.Contains(segments, s => s is { X: 150, Y: 150, Width: 20, Height: 20 });
    }

    [Fact]
    public void Segment_WorksOnLightBackground()
    {
        var pixels = Canvas((240, 240, 240));
        Rect(pixels, 50, 50, 20, 20, (90, 120, 60));

        var segments = GrainSegmenter.Segment(pixels, Size, Size);

        var grain = Assert.Single(segments);
        Assert.Equal((50, 50, 20, 20), (grain.X, grain.Y, grain.Width, grain.Height));
    }

    [Fact]
    public void Segment_EmptyBackground_ReturnsNoGrains()
    {
        Assert.Empty(GrainSegmenter.Segment(Canvas((10, 10, 10)), Size, Size));
    }
}