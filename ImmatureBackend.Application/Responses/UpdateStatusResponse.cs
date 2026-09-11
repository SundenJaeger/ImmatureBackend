using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

/// <summary>Response body for PATCH /api/replicates/{id}/status.</summary>
public sealed record UpdateStatusResponse
{
    public string Id { get; init; } = string.Empty;
    public ReviewStatus ReviewStatus { get; init; }
}