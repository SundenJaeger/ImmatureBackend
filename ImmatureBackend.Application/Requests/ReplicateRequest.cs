using Microsoft.AspNetCore.Http;

namespace ImmatureBackend.Application.Requests;

/// <summary>Multipart form-data payload for POST /api/replicate.</summary>
public sealed record ReplicateRequest
{
    /// <summary>Sample image, exactly 1024x1024 JPEG/PNG, up to 10 MB.</summary>
    public IFormFile Image { get; init; } = null!;

    public string TechnicianName { get; init; } = string.Empty;
    public string SampleId { get; init; } = string.Empty;

    /// <summary>AI-predicted grain boxes, JSON-encoded as a string (not a native array).</summary>
    public string AiPredictedGrains { get; init; } = string.Empty;

    /// <summary>Technician-confirmed grain boxes, JSON-encoded as a string (not a native array).</summary>
    public string ConfirmedGrains { get; init; } = string.Empty;

    /// <summary>Immature grain weight in grams. Must be greater than zero.</summary>
    public decimal? Weight { get; init; }
}