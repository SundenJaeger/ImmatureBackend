using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Domain.Models;
using Newtonsoft.Json;

namespace ImmatureBackend.Application.Services;

public class ReplicateService(IReplicateRepository repository, ICalculationService calculationService)
    : IReplicateService
{
    public async Task<List<ReplicateListItem>> GetAllReplicateListItemsAsync()
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

    public async Task<byte[]?> GetImage(Guid id)
    {
        return await repository.GetImageBytesAsync(id);
    }

    public async Task<UpdateStatusResponse> UpdateReviewStatus(Guid id, UpdateStatusRequest request)
    {
        if (request.Status != "accepted" && request.Status != "denied")
        {
            throw new InvalidDataException("Status must be 'accepted or 'denied'");
        }

        var updatedStatus = await repository.UpdateStatusAsync(id, request.Status);

        return new UpdateStatusResponse
        {
            Id = id.ToString(),
            ReviewStatus = updatedStatus
        };
    }

    public async Task<ReplicateResponse> CreateAsync(ReplicateRequest request, byte[] imageBytes)
    {
        var percentage = calculationService.CalculatePercentage(request.Weight);
        var grade = calculationService.AssignGrade(percentage);
        
        var entity = new ReplicateEntity
        {
            Id = Guid.NewGuid(),
            TechnicianName = request.TechnicianName,
            CreatedAt = DateTime.UtcNow,
            SampleId = request.SampleId,
            AiPredictedGrains = request.AiPredictedGrains,
            ConfirmedGrains = request.ConfirmedGrains,
            ImmatureWeight = request.Weight,
            Percentage = percentage,
            Grade = grade,
            OriginalImage = imageBytes,
            ReviewStatus = "unreviewed"
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