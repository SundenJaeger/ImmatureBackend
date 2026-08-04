using System.ComponentModel.DataAnnotations;

namespace ImmatureBackend.DTOs;

public sealed record ReplicateRequest
{
    [Required] public IFormFile Image { get; init; } = null!;
    [Required] public string TechnicianName { get; init; } = string.Empty;
    [Required] public string SampleId { get; init; } = string.Empty;
    [Required] public string AiPredictedGrains { get; init; } = string.Empty;
    [Required] public string ConfirmedGrains { get; init; } = string.Empty;
    [Required] public decimal Weight { get; init; }
}