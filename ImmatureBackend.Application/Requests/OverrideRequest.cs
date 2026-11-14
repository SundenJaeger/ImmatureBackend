namespace ImmatureBackend.Application.Requests;

public sealed record OverrideRequest
{
    public bool? Overridden { get; init; }
}