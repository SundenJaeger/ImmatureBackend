using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

public sealed record SampleRecordResponse
{
    public string? Season { get; init; }
    public string? IndexNumber { get; init; }
    public string? SelectionName { get; init; }
    public int? SampleNo { get; init; }
    public string SampleId { get; init; } = string.Empty;
    public int ReplicateCount { get; init; }
    public int GoodCount { get; init; }
    public ReviewStatus Status { get; init; }
    public decimal? AverageWeight { get; init; }
    public decimal? AveragePercentage { get; init; }
    public Grade? AverageGrade { get; init; }
    public bool? MeetsRecommendedValue { get; init; }
    public DateTime? LatestSubmittedAt { get; init; }
    public List<ReplicateListItem> Replicates { get; init; } = new();
}