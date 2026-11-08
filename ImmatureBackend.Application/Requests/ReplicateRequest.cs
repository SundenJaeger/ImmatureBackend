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

    /// <summary>Immature grain weight in grams. Must be greater than zero and at most 30.</summary>
    public decimal? Weight { get; init; }

    /// <summary>Season code, a four-digit year followed by DS or WS (for example 2026WS).</summary>
    public string Season { get; init; } = string.Empty;

    /// <summary>NCT index number of the entry.</summary>
    public string IndexNumber { get; init; } = string.Empty;

    /// <summary>NCT selection name of the entry.</summary>
    public string SelectionName { get; init; } = string.Empty;

    /// <summary>Which sample of the entry this is: 1 or 2.</summary>
    public int? SampleNo { get; init; }

    /// <summary>When the photo was captured (ISO-8601, UTC).</summary>
    public DateTimeOffset? CapturedAt { get; init; }

    /// <summary>When the technician submitted the record (ISO-8601, UTC). Not earlier than capturedAt.</summary>
    public DateTimeOffset? SubmittedAt { get; init; }
}