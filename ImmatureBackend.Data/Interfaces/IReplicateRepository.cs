using ImmatureBackend.Data.Models;

namespace ImmatureBackend.Data.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity);
    Task<List<ReplicateEntity>> GetAllAsync();
    Task<byte[]?> GetImageBytesAsync(Guid id);
    Task<string> UpdateStatusAsync(Guid id, string status);
}