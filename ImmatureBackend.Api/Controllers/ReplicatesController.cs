using System.Text.Json;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using ImmatureBackend.Data.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
public class ReplicatesController(IReplicateRepository replicateRepository, IAuthService authService) : ControllerBase
{
    [HttpGet("replicates")]
    public async Task<IActionResult> GetAll()
    {
        var entities = await replicateRepository.GetAllAsync();
        var dtos = entities.Select(e => new ReplicateListItem
        {
            Id = e.Id.ToString(),
            TechnicianName = e.TechnicianName,
            CreatedAt = e.CreatedAt,
            SampleId = e.SampleId,
            AiPredictedGrains = JsonSerializer.Deserialize<List<GrainBox>>(e.AiPredictedGrains) ?? new List<GrainBox>(),
            ConfirmedGrains = JsonSerializer.Deserialize<List<GrainBox>>(e.ConfirmedGrains) ?? new List<GrainBox>(),
            ImmatureWeight = e.ImmatureWeight,
            Percentage = e.Percentage,
            Grade = e.Grade,
            ReviewStatus = e.ReviewStatus
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("images/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(
        Guid id,
        [FromQuery] string? api_key = null,
        [FromHeader(Name = "X-API-Key")] string? xApiKey = null)
    {
        var apiKey = xApiKey ?? api_key;
        if (string.IsNullOrEmpty(apiKey) || !authService.IsValidKey(apiKey))
        {
            return Unauthorized(new { detail = "Invalid or missing API Key" });
        }

        var imageBytes = await replicateRepository.GetImageBytesAsync(id);

        if (imageBytes == null || imageBytes.Length == 0)
        {
            return NotFound(new { detail = "Image not found" });
        }

        return File(imageBytes, "image/jpeg");
    }

    [HttpPatch("replicates/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (request.Status != "accepted" && request.Status != "denied")
        {
            return BadRequest(new { detail = "Status must be 'accepted' or 'denied'" });
        }

        try
        {
            var updatedStatus = await replicateRepository.UpdateStatusAsync(id, request.Status);
            return Ok(new UpdateStatusResponse
            {
                Id = id.ToString(),
                ReviewStatus = updatedStatus
            });
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { detail = "Replicate record not found" });
        }
    }
}