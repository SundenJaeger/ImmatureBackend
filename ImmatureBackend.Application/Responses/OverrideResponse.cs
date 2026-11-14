using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

public sealed record OverrideResponse
{
    public string Id { get; init; } = string.Empty;
    public bool ManuallyOverridden { get; init; }
    public ReplicateQuality Quality { get; init; }
    public ReviewStatus ReviewStatus { get; init; }
}