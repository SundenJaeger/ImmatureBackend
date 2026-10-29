namespace ImmatureBackend.Application.Responses;

public sealed record ErrorResponse
{
    public string Message { get; init; } = string.Empty;
}