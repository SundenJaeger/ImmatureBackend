namespace ImmatureBackend.DTOs;

public sealed record ReplicateResponse
{
    public string Id { get; init; } = string.Empty;
    public decimal Percentage { get; init; }
    public string Grade { get; init; } = string.Empty;
}