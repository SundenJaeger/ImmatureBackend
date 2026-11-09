using System.Globalization;
using FluentResults;
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

                return entities.Select(entity => new ReplicateListItem
                {
                    Id = entity.Id.ToString(),
                    TechnicianName = entity.TechnicianName,
                    CreatedAt = entity.CreatedAt,
                    SampleId = entity.SampleId,
                    AiPredictedGrains = JsonConvert.DeserializeObject<List<GrainBox>>(entity.AiPredictedGrains) ??
                                        new List<GrainBox>(),
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
                    CapturedAt = entity.CapturedAt,
                    SubmittedAt = entity.SubmittedAt,
                    ImagePublicId = entity.ImagePublicId,
                    ImageSecureUrl = entity.ImageSecureUrl,
                    ProcessingSeconds = entity.ProcessingSeconds
                }).ToList();
            },
            token: cancellationToken
        );
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

        await cache.RemoveAsync(AllReplicatesCacheKey, token: cancellationToken);

        logger.LogInformation("Replicate {ReplicateId} review status set to {ReviewStatus}.", id, status);

        return new UpdateStatusResponse
        {
            Id = id.ToString(),
            ReviewStatus = status
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

        if (await repository.ExistsAsync(season, indexNumber, sampleNo, cancellationToken))
        {
            logger.LogWarning(
                "Replicate creation rejected: sample {SampleNo} already exists for season {Season}, index number {IndexNumber}.",
                sampleNo,
                season,
                indexNumber);

            return Result.Fail(new DuplicateSampleError(season, indexNumber, sampleNo));
        }

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
                "Replicate creation rejected by the unique index: sample {SampleNo}, season {Season}, index number {IndexNumber}.",
                sampleNo,
                season,
                indexNumber);

            return Result.Fail(new DuplicateSampleError(season, indexNumber, sampleNo));
        }

        await cache.RemoveAsync(AllReplicatesCacheKey, token: cancellationToken);

        logger.LogInformation(
            "Replicate {ReplicateId} created for sample {SampleId} by {TechnicianName}: {Weight} g -> {Percentage}% ({Grade}).",
            saved.Id.ToString(),
            saved.SampleId,
            saved.TechnicianName,
            saved.ImmatureWeight.ToString(CultureInfo.InvariantCulture),
            saved.Percentage.ToString(CultureInfo.InvariantCulture),
            saved.Grade);

        return new ReplicateResponse
        {
            Id = saved.Id.ToString(),
            Percentage = saved.Percentage,
            Grade = saved.Grade,
            MeetsRecommendedValue = saved.MeetsRecommendedValue
        };
    }
}