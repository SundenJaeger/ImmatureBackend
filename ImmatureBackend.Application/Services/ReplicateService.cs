using System.Globalization;
using FluentResults;
using ImmatureBackend.Application.Constants;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Exceptions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ZiggyCreatures.Caching.Fusion;

namespace ImmatureBackend.Application.Services;

public class ReplicateService(
    IReplicateRepository repository,
    ICalculationService calculationService,
    IImageService imageService,
    IFusionCache cache,
    ILogger<ReplicateService> logger)
    : IReplicateService
{
    private const string AllReplicatesCacheKey = "replicates:all";

    public async Task<IReadOnlyList<ReplicateListItem>> GetAllReplicateListItemsAsync(
        CancellationToken cancellationToken = default)
    {
        return await cache.GetOrSetAsync<IReadOnlyList<ReplicateListItem>>(
            AllReplicatesCacheKey,
            async (_, token) =>
            {
                var entities = await repository.GetAllAsync(token);

                return entities.Select(ToListItem).ToList();
            },
            token: cancellationToken
        );
    }

    public async Task<IReadOnlyList<SampleRecordResponse>> GetAllSampleRecordsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await GetAllReplicateListItemsAsync(cancellationToken);

        return items
            .GroupBy(item => (item.Season, item.IndexNumber, item.SampleNo))
            .Select(group => ToSampleRecord(group.OrderBy(item => item.ReplicateNo).ToList()))
            .OrderByDescending(record => record.LatestSubmittedAt)
            .ToList();
    }

    public async Task<Result<UpdateStatusResponse>> UpdateReviewStatus(Guid id, UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var status = Enum.Parse<ReviewStatus>(request.Status!, true);
        var updatedStatus = await repository.UpdateStatusAsync(id, status, cancellationToken);

        if (!updatedStatus)
        {
            logger.LogWarning(
                "Failed to update review status for replicate {ReplicateId}: replicate not found.",
                id);

            return Result.Fail(new ReplicateNotFoundError(id));
        }
        
        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            await AutoAcceptSampleAsync(entity.Season, entity.IndexNumber, entity.SampleNo, cancellationToken);
        }

        await cache.RemoveAsync(AllReplicatesCacheKey, token: cancellationToken);

        logger.LogInformation("Replicate {ReplicateId} review status set to {ReviewStatus}.", id, status);

        return new UpdateStatusResponse
        {
            Id = id.ToString(),
            ReviewStatus = status
        };
    }

    public async Task<Result<OverrideResponse>> UpdateManualOverride(Guid id, OverrideRequest request,
        CancellationToken cancellationToken = default)
    {
        var overridden = request.Overridden!.Value;

        if (!await repository.UpdateManualOverrideAsync(id, overridden, cancellationToken))
        {
            logger.LogWarning(
                "Failed to update manual override for replicate {ReplicateId}: replicate not found.",
                id);

            return Result.Fail(new ReplicateNotFoundError(id));
        }

        var entity = await repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return Result.Fail(new ReplicateNotFoundError(id));
        }

        await AutoAcceptSampleAsync(entity.Season, entity.IndexNumber, entity.SampleNo, cancellationToken);
        
        entity = await repository.GetByIdAsync(id, cancellationToken) ?? entity;

        await cache.RemoveAsync(AllReplicatesCacheKey, token: cancellationToken);

        logger.LogInformation("Replicate {ReplicateId} manual override set to {Overridden}.", id, overridden);

        var item = ToListItem(entity);

        return new OverrideResponse
        {
            Id = item.Id,
            ManuallyOverridden = item.ManuallyOverridden,
            Quality = item.Quality,
            ReviewStatus = item.ReviewStatus
        };
    }

    public async Task<Result<ReplicateResponse>> CreateAsync(ReplicateRequest request,
        CancellationToken cancellationToken = default)
    {
        var season = request.Season.Trim();
        var indexNumber = request.IndexNumber.Trim();
        var selectionName = request.SelectionName.Trim();
        var sampleNo = request.SampleNo!.Value;

        logger.LogInformation(
            "Creating replicate for sample {SampleId} by Technician {TechnicianName}",
            request.SampleId,
            request.TechnicianName);

        var percentage = calculationService.CalculatePercentage(request.Weight!.Value);
        var grade = calculationService.AssignGrade(percentage);

        var result = await imageService.UploadAsync(request.Image, cancellationToken);
        if (result.IsFailed)
        {
            logger.LogWarning(
                "Replicate creation aborted for sample {SampleId}: image upload failed. {Errors}",
                request.SampleId,
                string.Join("; ", result.Errors.Select(e => e.Message))
            );

            return result.ToResult<ReplicateResponse>();
        }

        var (publicId, secureUrl) = result.Value;

        var entity = new ReplicateEntity
        {
            TechnicianName = request.TechnicianName,
            SampleId = request.SampleId,
            AiPredictedGrains = request.AiPredictedGrains,
            ConfirmedGrains = request.ConfirmedGrains,
            ImmatureWeight = request.Weight!.Value,
            Percentage = percentage,
            Grade = grade,
            ImagePublicId = publicId,
            ImageSecureUrl = secureUrl,
            ReviewStatus = ReviewStatus.Review,
            Season = season,
            IndexNumber = indexNumber,
            SelectionName = selectionName,
            SampleNo = sampleNo,
            ManuallyOverridden = request.ManuallyOverridden,
            CapturedAt = request.CapturedAt!.Value.UtcDateTime,
            SubmittedAt = request.SubmittedAt!.Value.UtcDateTime,
            MeetsRecommendedValue = calculationService.MeetsRecommendedValue(percentage),
            ProcessingSeconds =
                calculationService.CalculateProcessingSeconds(request.CapturedAt!.Value, request.SubmittedAt!.Value)
        };

        ReplicateEntity saved;
        try
        {
            saved = await repository.CreateAsync(entity, cancellationToken);
        }
        catch (DuplicateReplicateException)
        {
            logger.LogWarning(
                "Replicate creation gave up assigning a replicate number: sample {SampleNo}, season {Season}, index number {IndexNumber}.",
                sampleNo,
                season,
                indexNumber);

            return Result.Fail(new ReplicateNumberConflictError(season, indexNumber, sampleNo));
        }

        var accepted = await AutoAcceptSampleAsync(season, indexNumber, sampleNo, cancellationToken);

        await cache.RemoveAsync(AllReplicatesCacheKey, token: cancellationToken);

        var item = ToListItem(saved);

        logger.LogInformation(
            "Replicate {ReplicateId} ({ReplicateLabel}) created for sample {SampleId} by {TechnicianName}: {Weight} g -> {Percentage}% ({Grade}), quality {Quality}.",
            saved.Id.ToString(),
            item.ReplicateLabel,
            saved.SampleId,
            saved.TechnicianName,
            saved.ImmatureWeight.ToString(CultureInfo.InvariantCulture),
            saved.Percentage.ToString(CultureInfo.InvariantCulture),
            saved.Grade,
            item.Quality);

        return new ReplicateResponse
        {
            Id = saved.Id.ToString(),
            Percentage = saved.Percentage,
            Grade = saved.Grade,
            MeetsRecommendedValue = saved.MeetsRecommendedValue,
            ReplicateNo = saved.ReplicateNo,
            ReplicateLabel = item.ReplicateLabel,
            Confidence = item.Confidence,
            Quality = item.Quality,
            ReviewStatus = accepted.Contains(saved.Id) ? ReviewStatus.Accepted : saved.ReviewStatus
        };
    }
    
    private async Task<IReadOnlyCollection<Guid>> AutoAcceptSampleAsync(string season, string indexNumber,
        int sampleNo, CancellationToken cancellationToken)
    {
        var entities = await repository.GetBySampleAsync(season, indexNumber, sampleNo, cancellationToken);

        var acceptedCount = entities.Count(entity => entity.ReviewStatus == ReviewStatus.Accepted);
        var missing = ReplicateRules.RequiredAcceptedReplicates - acceptedCount;
        if (missing <= 0)
        {
            return [];
        }

        var candidates = entities
            .Where(entity => entity.ReviewStatus == ReviewStatus.Review
                             && ToListItem(entity).Quality == ReplicateQuality.Good)
            .OrderBy(entity => entity.ReplicateNo)
            .ToList();

        if (candidates.Count < missing)
        {
            return [];
        }

        var ids = candidates.Take(missing).Select(entity => entity.Id).ToList();
        await repository.AcceptAsync(ids, cancellationToken);

        logger.LogInformation(
            "Sample {SampleNo}, season {Season}, index number {IndexNumber} auto-accepted replicates {ReplicateIds}.",
            sampleNo,
            season,
            indexNumber,
            string.Join(", ", ids));

        return ids;
    }

    private ReplicateListItem ToListItem(ReplicateEntity entity)
    {
        var aiGrains = JsonConvert.DeserializeObject<List<GrainBox>>(entity.AiPredictedGrains) ??
                       new List<GrainBox>();
        var confidence = calculationService.CalculateConfidence(aiGrains);

        return new ReplicateListItem
        {
            Id = entity.Id.ToString(),
            TechnicianName = entity.TechnicianName,
            CreatedAt = entity.CreatedAt,
            SampleId = entity.SampleId,
            AiPredictedGrains = aiGrains,
            ConfirmedGrains = JsonConvert.DeserializeObject<List<GrainBox>>(entity.ConfirmedGrains) ??
                              new List<GrainBox>(),
            ImmatureWeight = entity.ImmatureWeight,
            Percentage = entity.Percentage,
            Grade = entity.Grade,
            ReviewStatus = entity.ReviewStatus,
            MeetsRecommendedValue = entity.MeetsRecommendedValue,
            Season = entity.Season,
            IndexNumber = entity.IndexNumber,
            SelectionName = entity.SelectionName,
            SampleNo = entity.SampleNo,
            ReplicateNo = entity.ReplicateNo,
            ReplicateLabel =
                $"{entity.SampleId}-{entity.ReplicateNo.ToString(ReplicateRules.ReplicateNoFormat, CultureInfo.InvariantCulture)}",
            Confidence = confidence,
            Quality = calculationService.AssessQuality(confidence, entity.ManuallyOverridden),
            ManuallyOverridden = entity.ManuallyOverridden,
            CapturedAt = entity.CapturedAt,
            SubmittedAt = entity.SubmittedAt,
            ImagePublicId = entity.ImagePublicId,
            ImageSecureUrl = entity.ImageSecureUrl,
            ProcessingSeconds = entity.ProcessingSeconds
        };
    }

    private SampleRecordResponse ToSampleRecord(IReadOnlyList<ReplicateListItem> replicates)
    {
        var first = replicates[0];
        var accepted = replicates.Where(item => item.ReviewStatus == ReviewStatus.Accepted).ToList();
        var isAccepted = accepted.Count >= ReplicateRules.RequiredAcceptedReplicates;

        decimal? averageWeight = null;
        decimal? averagePercentage = null;
        Grade? averageGrade = null;
        bool? meetsRecommendedValue = null;

        if (isAccepted)
        {
            var weight =
                calculationService.CalculateAverageWeight(accepted.Select(item => item.ImmatureWeight).ToList());
            var percentage = calculationService.CalculatePercentage(weight);

            averageWeight = weight;
            averagePercentage = percentage;
            averageGrade = calculationService.AssignGrade(percentage);
            meetsRecommendedValue = calculationService.MeetsRecommendedValue(percentage);
        }

        return new SampleRecordResponse
        {
            Season = first.Season,
            IndexNumber = first.IndexNumber,
            SelectionName = first.SelectionName,
            SampleNo = first.SampleNo,
            SampleId = first.SampleId,
            ReplicateCount = replicates.Count,
            GoodCount = replicates.Count(item => item.Quality == ReplicateQuality.Good),
            Status = isAccepted ? ReviewStatus.Accepted : ReviewStatus.Review,
            AverageWeight = averageWeight,
            AveragePercentage = averagePercentage,
            AverageGrade = averageGrade,
            MeetsRecommendedValue = meetsRecommendedValue,
            LatestSubmittedAt = replicates.Max(item => item.SubmittedAt),
            Replicates = replicates.ToList()
        };
    }
}