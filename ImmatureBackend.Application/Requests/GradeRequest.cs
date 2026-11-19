namespace ImmatureBackend.Application.Requests;

/// <summary>Query parameters for GET /api/grade.</summary>
public sealed record GradeRequest
{
    /// <summary>Immature grain weight in grams. Must be greater than zero and at most 30.</summary>
    public decimal? Weight { get; init; }
}
