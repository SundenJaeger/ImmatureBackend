using ImmatureBackend.Application.Constants;
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
        for (var attempt = 1;; attempt++)
        {
            var highest = await context.ReplicateEntities
                .AsNoTracking()
                .Where(row => row.Season == entity.Season
                              && row.IndexNumber == entity.IndexNumber
                              && row.SampleNo == entity.SampleNo)
                .MaxAsync(row => (int?)row.ReplicateNo, cancellationToken);

            entity.ReplicateNo = (highest ?? 0) + 1;
            context.ReplicateEntities.Add(entity);

            try
            {
                await context.SaveChangesAsync(cancellationToken);

                return entity;
            }
            catch (DbUpdateException exception) when (IsDuplicateReplicateNo(exception))
            {
                context.Entry(entity).State = EntityState.Detached;

                if (attempt >= ReplicateRules.MaxReplicateNoAttempts)
                {
                    throw new DuplicateReplicateException(exception);
                }
            }
        }
    }

    public async Task<IReadOnlyList<ReplicateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ReplicateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ReplicateEntity>> GetBySampleAsync(string season, string indexNumber,
        int sampleNo, CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .AsNoTracking()
            .Where(entity => entity.Season == season
                             && entity.IndexNumber == indexNumber
                             && entity.SampleNo == sampleNo)
            .OrderBy(entity => entity.ReplicateNo)
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

    public async Task<bool> UpdateManualOverrideAsync(Guid id, bool manuallyOverridden,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await context.ReplicateEntities
            .Where(entity => entity.Id == id)
            .ExecuteUpdateAsync(builder => builder
                    .SetProperty(entity => entity.ManuallyOverridden, manuallyOverridden),
                cancellationToken);

        return rowsAffected > 0;
    }

    public async Task<int> AcceptAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await context.ReplicateEntities
            .Where(entity => ids.Contains(entity.Id) && entity.ReviewStatus == ReviewStatus.Review)
            .ExecuteUpdateAsync(builder => builder
                    .SetProperty(entity => entity.ReviewStatus, ReviewStatus.Accepted),
                cancellationToken);
    }

    private static bool IsDuplicateReplicateNo(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ReplicateEntityConfiguration.UniqueSampleIndexName
        };
    }
}