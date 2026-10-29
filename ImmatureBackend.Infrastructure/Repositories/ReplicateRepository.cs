using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImmatureBackend.Infrastructure.Repositories;

public class ReplicateRepository(AppDbContext context) : IReplicateRepository
{
    public async Task<ReplicateEntity> CreateAsync(ReplicateEntity entity,
        CancellationToken cancellationToken = default)
    {
        context.ReplicateEntities.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<IReadOnlyList<ReplicateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<byte[]?> GetImageBytesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .Where(entity => entity.Id == id)
            .Select(entity => entity.OriginalImage)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> UpdateStatusAsync(Guid id, ReviewStatus status,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await context.ReplicateEntities
            .Where(entity => entity.Id == id)
            .ExecuteUpdateAsync(builder => builder
                    .SetProperty(entity => entity.ReviewStatus, status),
                cancellationToken);

        return rowsAffected > 0;
    }
}