namespace ImmatureBackend.DTOs;

public sealed record PredictResponse
{
    public string ImageId { get; init; } = string.Empty;
    public List<GrainBox> Grains { get; init; } = new();
}