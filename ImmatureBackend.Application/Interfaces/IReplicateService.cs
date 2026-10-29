using FluentResults;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateService
{
    Task<IReadOnlyList<ReplicateListItem>> GetAllReplicateListItemsAsync(CancellationToken cancellationToken = default);
    Task<Result<(byte[] bytes, string contentType)>> GetImage(Guid id, CancellationToken cancellationToken = default);
    Task<Result<UpdateStatusResponse>> UpdateReviewStatus(Guid id, UpdateStatusRequest request, CancellationToken cancellationToken = default);
    Task<Result<ReplicateResponse>> CreateAsync(ReplicateRequest request, CancellationToken cancellationToken = default);
}