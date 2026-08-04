using ImmatureBackend.Models;

namespace ImmatureBackend.Repositories.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity);
    Task<List<ReplicateEntity>> GetAllAsync();
    Task<byte[]?> GetImageBytesAsync(Guid id);
    Task<string> UpdateStatusAsync(Guid id, string status);
}