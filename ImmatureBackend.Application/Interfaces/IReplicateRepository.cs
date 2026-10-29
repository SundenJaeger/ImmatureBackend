using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReplicateEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateStatusAsync(Guid id, ReviewStatus status, CancellationToken cancellationToken = default);
}