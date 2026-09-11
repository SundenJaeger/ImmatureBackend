using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Requests;

/// <summary>Multipart form-data payload for POST /api/predict.</summary>
public sealed record PredictRequest
{
    /// <summary>Sample image, exactly 1024x1024 JPEG/PNG, up to 10 MB.</summary>
    public IFormFile Image { get; init; } = null!;

    public string TechnicianName { get; init; } = string.Empty;
    public string SampleId { get; init; } = string.Empty;
}