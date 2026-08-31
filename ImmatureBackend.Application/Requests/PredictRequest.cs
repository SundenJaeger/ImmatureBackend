using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Requests;

public sealed record PredictRequest
{
    public IFormFile Image { get; init; } = null!;
    public string TechnicianName { get; init; } = string.Empty;
    public string SampleId { get; init; } = string.Empty;
}