using ImmatureBackend.Domain.Models;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity);
    Task<List<ReplicateEntity>> GetAllAsync();
    Task<byte[]?> GetImageBytesAsync(Guid id);
    Task<string> UpdateStatusAsync(Guid id, string status);
}