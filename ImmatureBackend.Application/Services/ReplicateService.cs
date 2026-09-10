using FileSignatures;
using FileSignatures.Formats;
using ImmatureBackend.Application.Exceptions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Models;
using Newtonsoft.Json;

namespace ImmatureBackend.Application.Services;

public class ReplicateService(
    IReplicateRepository repository,
    ICalculationService calculationService,
    IFileFormatInspector fileFormatInspector)
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

    public async Task<(byte[] bytes, string contentType)> GetImage(Guid id)
    {
        var image = await repository.GetImageBytesAsync(id);

        if (image is null || image.Length == 0)
        {
            throw new ImageNotFoundException("Image not found.");
        }

        await using var stream = new MemoryStream(image);

        var inspect = fileFormatInspector.DetermineFileFormat(stream);

        var contentType = inspect switch
        {
            Jpeg => "image/jpeg",
            Png => "image/png",
            _ => throw new InvalidImageException("Unsupported image format.")
        };

        return (image, contentType);
    }

    public async Task<UpdateStatusResponse> UpdateReviewStatus(Guid id, UpdateStatusRequest request)
    {
        var status = Enum.Parse<ReviewStatus>(request.Status!, true);
        var updatedStatus = await repository.UpdateStatusAsync(id, status);

        if (!updatedStatus)
        {
            throw new ReplicateNotFoundException("Replicate not found.");
        }

        return new UpdateStatusResponse
        {
            Id = id.ToString(),
            ReviewStatus = status
        };
    }

    public async Task<ReplicateResponse> CreateAsync(ReplicateRequest request)
    {
        var percentage = calculationService.CalculatePercentage(request.Weight!.Value);
        var grade = calculationService.AssignGrade(percentage);

        var image = request.Image;

        await using var readStream = image.OpenReadStream();
        using var memStream = new MemoryStream();

        await readStream.CopyToAsync(memStream);

        var inspect = fileFormatInspector.DetermineFileFormat(memStream);

        if (inspect is not (Jpeg or Png))
        {
            throw new InvalidImageException("Image can only be JPEG or PNG.");
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

        return new ReplicateResponse
        {
            Id = saved.Id.ToString(),
            Percentage = saved.Percentage,
            Grade = saved.Grade
        };
    }
}