namespace ImmatureBackend.DTOs;

public sealed record UpdateStatusRequest
{
    public string Status { get; init; } = string.Empty;
}