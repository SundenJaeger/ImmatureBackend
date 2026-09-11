namespace ImmatureBackend.Application.Responses;

/// <summary>Response body for POST /api/predict.</summary>
public sealed record PredictResponse
{
    public string ImageId { get; init; } = string.Empty;
    public List<GrainBox> Grains { get; init; } = new();
}