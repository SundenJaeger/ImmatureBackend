using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

/// <summary>Response body for GET /api/grade.</summary>
public sealed record GradeResponse
{
    /// <summary>The weight that was graded, in grams.</summary>
    public decimal Weight { get; init; }

    /// <summary>Immature percentage, rounded to 1 decimal place (half away from zero).</summary>
    public decimal Percentage { get; init; }

    /// <summary>One of "Pr", "G1", "G2", "G3", "Not Classified".</summary>
    public Grade Grade { get; init; }

    /// <summary>True when the percentage is below the recommended value of 2.0%.</summary>
    public bool MeetsRecommendedValue { get; init; }
}
