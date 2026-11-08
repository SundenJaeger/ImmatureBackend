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
}