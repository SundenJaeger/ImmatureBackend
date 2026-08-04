using ImmatureBackend.Enums;

namespace ImmatureBackend.DTOs;

public sealed record UpdateStatusRequest
{
    public ReviewStatus Status { get; init; }
}