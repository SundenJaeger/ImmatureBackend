namespace ImmatureBackend.Application.Requests;

/// <summary>Body for PATCH /api/replicates/{id}/status.</summary>
public sealed record UpdateStatusRequest
{
    /// <summary>New review status: "review", "accepted", "rejected", or "retraining".</summary>
    public string? Status { get; init; }
}