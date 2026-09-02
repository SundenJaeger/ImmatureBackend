using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity);
    Task<List<ReplicateEntity>> GetAllAsync();
    Task<byte[]?> GetImageBytesAsync(Guid id);
    Task<ReviewStatus?> UpdateStatusAsync(Guid id, ReviewStatus status);
}