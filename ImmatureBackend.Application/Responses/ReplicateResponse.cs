using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

/// <summary>Response body for POST /api/replicate.</summary>
public sealed record ReplicateResponse
{
    public string Id { get; init; } = string.Empty;

    /// <summary>Immature percentage, rounded to 1 decimal place (half away from zero).</summary>
    public decimal Percentage { get; init; }

    /// <summary>One of "Pr", "G1", "G2", "G3", "Not classified".</summary>
    public Grade Grade { get; init; }

    /// <summary>True when the percentage is below the recommended value of 2.0%.</summary>
    public bool MeetsRecommendedValue { get; init; }

    /// <summary>Replicate number assigned within the sample (1, 2, 3 ...).</summary>
    public int ReplicateNo { get; init; }

    /// <summary>Sample id plus the replicate suffix, for example "S1-02".</summary>
    public string ReplicateLabel { get; init; } = string.Empty;

    /// <summary>Mean AI box confidence (0-1), null when unavailable.</summary>
    public decimal? Confidence { get; init; }

    public ReplicateQuality Quality { get; init; }

    /// <summary>"accepted" when this submission completed the sample and it was auto-accepted, otherwise "review".</summary>
    public ReviewStatus ReviewStatus { get; init; }
}