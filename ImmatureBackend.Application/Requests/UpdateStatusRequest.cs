namespace ImmatureBackend.Application.Requests;

public sealed record UpdateStatusRequest
{
    public string Status { get; init; } = string.Empty;
}