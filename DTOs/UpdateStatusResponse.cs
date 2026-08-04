namespace ImmatureBackend.DTOs;

public sealed record UpdateStatusResponse
{
    public string Id { get; init; } = string.Empty;
    public string ReviewStatus { get; init; } = string.Empty;
}