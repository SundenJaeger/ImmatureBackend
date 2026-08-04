using ImmatureBackend.Enums;

namespace ImmatureBackend.DTOs;

public sealed record UpdateStatusResponse
{
    public string Id { get; init; } = string.Empty;
    public ReviewStatus ReviewStatus { get; init; }
}