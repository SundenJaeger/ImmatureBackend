namespace ImmatureBackend.Application.Responses;

public sealed record GrainBox
{
    public int X { get; init; }
    public int Y { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public double? Confidence { get; init; }
    public string? Action { get; init; }
}