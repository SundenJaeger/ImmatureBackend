using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateService
{
    Task<List<ReplicateListItem>> GetAllReplicateListItemsAsync();
    Task<byte[]> GetImage(Guid id);
    Task<UpdateStatusResponse> UpdateReviewStatus(Guid id, UpdateStatusRequest request);
    Task<ReplicateResponse> CreateAsync(ReplicateRequest request, byte[] imageBytes);
}