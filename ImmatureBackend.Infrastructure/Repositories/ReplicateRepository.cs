using ImmatureBackend.Application.Exceptions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using ImmatureBackend.Infrastructure.Persistence;
using ImmatureBackend.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ImmatureBackend.Infrastructure.Repositories;

public class ReplicateRepository(AppDbContext context) : IReplicateRepository
{
    public async Task<ReplicateEntity> CreateAsync(ReplicateEntity entity,
        CancellationToken cancellationToken = default)
    {
        context.ReplicateEntities.Add(entity);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsDuplicateSample(exception))
        {
            context.Entry(entity).State = EntityState.Detached;

            throw new DuplicateReplicateException(exception);
        }

        return entity;
    }

    public async Task<bool> ExistsAsync(string season, string indexNumber, int sampleNo,
        CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .AnyAsync(entity => entity.Season == season
                                && entity.IndexNumber == indexNumber
                                && entity.SampleNo == sampleNo,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ReplicateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

    private static bool IsDuplicateSample(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ReplicateEntityConfiguration.UniqueSampleIndexName
        };
    }
}