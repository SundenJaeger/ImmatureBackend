using System.ComponentModel.DataAnnotations;

namespace ImmatureBackend.DTOs;

public sealed record PredictRequest
{
    [Required] public IFormFile Image { get; init; } = null!;
    [Required] public string TechnicianName { get; init; } = string.Empty;
    [Required] public string SampleId { get; init; } = string.Empty;
}