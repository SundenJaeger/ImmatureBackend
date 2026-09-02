using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

public sealed record UpdateStatusResponse
{
    public string Id { get; init; } = string.Empty;
    public ReviewStatus ReviewStatus { get; init; }
}