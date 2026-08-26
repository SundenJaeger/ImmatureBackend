using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Requests;

public sealed record PredictRequest
{
    [Required] public IFormFile Image { get; init; } = null!;
    [Required] public string TechnicianName { get; init; } = string.Empty;
    [Required] public string SampleId { get; init; } = string.Empty;
}