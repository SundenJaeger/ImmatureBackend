using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Data.Interfaces;
using ImmatureBackend.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
public class PredictController(
    IGrainDetector grainDetector,
    ICalculationService calculationService,
    IReplicateRepository replicateRepository) : ControllerBase
{
    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromForm] PredictRequest request)
    {
        using var ms = new MemoryStream();
        await request.Image.CopyToAsync(ms);
        var imageBytes = ms.ToArray();

        var allGrains = grainDetector.Detect(imageBytes);
        var filteredGrains = allGrains
            .Where(box => box.Confidence is >= 0.5)
            .ToList();

        var imageId = Guid.NewGuid().ToString();

        return Ok(new PredictResponse
        {
            ImageId = imageId,
            Grains = filteredGrains
        });
    }

    [HttpPost("replicate")]
    public async Task<IActionResult> Replicate([FromForm] ReplicateRequest model)
    {
        var percentage = calculationService.CalculatePercentage(model.Weight);
        var grade = calculationService.AssignGrade(percentage);

        byte[] imageBytes;
        using (var ms = new MemoryStream())
        {
            await model.Image.CopyToAsync(ms);
            imageBytes = ms.ToArray();
        }

        var entity = new ReplicateEntity
        {
            Id = Guid.NewGuid(),
            TechnicianName = model.TechnicianName,
            CreatedAt = DateTime.UtcNow,
            SampleId = model.SampleId,
            AiPredictedGrains = model.AiPredictedGrains,
            ConfirmedGrains = model.ConfirmedGrains,
            ImmatureWeight = model.Weight,
            Percentage = percentage,
            Grade = grade,
            OriginalImage = imageBytes,
            ReviewStatus = "unreviewed"
        };

        var saved = await replicateRepository.CreateAsync(entity);

        return Ok(new ReplicateResponse()
        {
            Id = saved.Id.ToString(),
            Percentage = saved.Percentage,
            Grade = saved.Grade
        });
    }
}