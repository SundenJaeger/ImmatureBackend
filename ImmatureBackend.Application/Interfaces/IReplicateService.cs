using FluentResults;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateService
{
    Task<IReadOnlyList<ReplicateListItem>> GetAllReplicateListItemsAsync();
    Task<Result<(byte[] bytes, string contentType)>> GetImage(Guid id);
    Task<Result<UpdateStatusResponse>> UpdateReviewStatus(Guid id, UpdateStatusRequest request);
    Task<Result<ReplicateResponse>> CreateAsync(ReplicateRequest request);
}