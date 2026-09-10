using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ImmatureBackend.Infrastructure.Repositories;

public class ReplicateRepository(AppDbContext context) : IReplicateRepository
{
    public async Task<ReplicateEntity> CreateAsync(ReplicateEntity entity)
    {
        context.ReplicateEntities.Add(entity);
        await context.SaveChangesAsync();

        return entity;
    }

    public async Task<IReadOnlyList<ReplicateEntity>> GetAllAsync()
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<byte[]?> GetImageBytesAsync(Guid id)
    {
        return await context.ReplicateEntities
            .Where(entity => entity.Id == id)
            .Select(entity => entity.OriginalImage)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateStatusAsync(Guid id, ReviewStatus status)
    {
        var rowsAffected = await context.ReplicateEntities
            .Where(entity => entity.Id == id)
            .ExecuteUpdateAsync(builder => builder
                .SetProperty(entity => entity.ReviewStatus, status));

        return rowsAffected > 0;
    }
}