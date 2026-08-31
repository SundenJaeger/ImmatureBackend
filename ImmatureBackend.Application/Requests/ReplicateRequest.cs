using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Requests;

public sealed record ReplicateRequest
{
    public IFormFile Image { get; init; } = null!;
    public string TechnicianName { get; init; } = string.Empty;
    public string SampleId { get; init; } = string.Empty;
    public string AiPredictedGrains { get; init; } = string.Empty;
    public string ConfirmedGrains { get; init; } = string.Empty;
    public decimal? Weight { get; init; }
}