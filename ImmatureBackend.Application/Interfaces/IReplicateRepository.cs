using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;

namespace ImmatureBackend.Application.Interfaces;

public interface IReplicateRepository
{
    Task<ReplicateEntity> CreateAsync(ReplicateEntity entity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReplicateEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ReplicateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<ReplicateEntity>> GetBySampleAsync(string season, string indexNumber, int sampleNo,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(Guid id, ReviewStatus status, CancellationToken cancellationToken = default);

    Task<bool> UpdateManualOverrideAsync(Guid id, bool manuallyOverridden,
        CancellationToken cancellationToken = default);
    
    Task<int> AcceptAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}