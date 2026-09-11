namespace ImmatureBackend.Application.Responses;

/// <summary>A single grain bounding box, in the 1024x1024 image coordinate space.</summary>
public sealed record GrainBox
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    /// <summary>Detection confidence (0-1). Only boxes &gt;= 0.5 come back from /api/predict.</summary>
    public double? Confidence { get; init; }

    /// <summary>Reserved for future use; currently always null.</summary>
    public string? Action { get; init; }
}