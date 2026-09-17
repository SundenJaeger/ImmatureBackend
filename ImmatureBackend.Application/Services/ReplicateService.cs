using System.Globalization;
using FileSignatures;
using FileSignatures.Formats;
using FluentResults;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ImmatureBackend.Application.Services;

public class ReplicateService(
    IReplicateRepository repository,
    ICalculationService calculationService,
    IFileFormatInspector fileFormatInspector,
    ILogger<ReplicateService> logger)
    : IReplicateService
{
    public async Task<IReadOnlyList<ReplicateListItem>> GetAllReplicateListItemsAsync()
    {
        var entities = await repository.GetAllAsync();

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
            ReviewStatus = entity.ReviewStatus
        }).ToList();
    }

    public async Task<Result<(byte[] bytes, string contentType)>> GetImage(Guid id)
    {
        var image = await repository.GetImageBytesAsync(id);

        if (image is null || image.Length == 0)
        {
            logger.LogWarning(
                "Image not found for replicate {ReplicateId}.",
                id);

            return Result.Fail(new ImageNotFoundError(id));
        }

        await using var stream = new MemoryStream(image);

        var inspect = fileFormatInspector.DetermineFileFormat(stream);

        var contentType = inspect switch
        {
            Jpeg => "image/jpeg",
            Png => "image/png",
            _ => null
        };

        if (contentType is null)
        {
            logger.LogError(
                "Replicate {ReplicateId} contains an unsupported or invalid stored image format.",
                id);

            return Result.Fail(new InvalidImageError());
        }

        return (image, contentType);
    }

    public async Task<Result<UpdateStatusResponse>> UpdateReviewStatus(Guid id, UpdateStatusRequest request)
    {
        var status = Enum.Parse<ReviewStatus>(request.Status!, true);
        var updatedStatus = await repository.UpdateStatusAsync(id, status);

        if (!updatedStatus)
        {
            logger.LogWarning(
                "Failed to update review status for replicate {ReplicateId}: replicate not found.",
                id);
            
            return Result.Fail(new ReplicateNotFoundError(id));
        }

        logger.LogInformation("Replicate {ReplicateId} review status set to {ReviewStatus}.", id, status);

        return new UpdateStatusResponse
        {
            Id = id.ToString(),
            ReviewStatus = status
        };
    }

    public async Task<Result<ReplicateResponse>> CreateAsync(ReplicateRequest request)
    {
        logger.LogInformation(
            "Creating replicate for sample {SampleId} by Technician {TechnicianName}",
            request.SampleId,
            request.TechnicianName);

        var percentage = calculationService.CalculatePercentage(request.Weight!.Value);
        var grade = calculationService.AssignGrade(percentage);

        var image = request.Image;

        await using var readStream = image.OpenReadStream();
        using var memStream = new MemoryStream();

        await readStream.CopyToAsync(memStream);

        var inspect = fileFormatInspector.DetermineFileFormat(memStream);

        if (inspect is not (Jpeg or Png))
        {
            logger.LogWarning(
                "Replicate creation rejected for sample {SampleId}: unsupported image format.",
                request.SampleId);

            return Result.Fail(new InvalidImageError());
        }

        var imageBytes = memStream.ToArray();


        var entity = new ReplicateEntity
        {
            Id = Guid.NewGuid(),
            TechnicianName = request.TechnicianName,
            CreatedAt = DateTime.UtcNow,
            SampleId = request.SampleId,
            AiPredictedGrains = request.AiPredictedGrains,
            ConfirmedGrains = request.ConfirmedGrains,
            ImmatureWeight = request.Weight!.Value,
            Percentage = percentage,
            Grade = grade,
            OriginalImage = imageBytes,
            ReviewStatus = ReviewStatus.Review
        };

        var saved = await repository.CreateAsync(entity);

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
            Grade = saved.Grade
        };
    }
}