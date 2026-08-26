using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReplicatesController(IReplicateService replicateService) : ControllerBase
{
    [HttpGet("replicates")]
    public async Task<IActionResult> GetAll()
    {
        var entities = await replicateService.GetAllReplicateListItemsAsync();

        return Ok(entities);
    }

    [HttpGet("images/{id}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var imageBytes = await replicateService.GetImage(id);

        if (imageBytes is null || imageBytes.Length == 0)
        {
            return NotFound(new { detail = "Image not found" });
        }

        return File(imageBytes, "image/jpeg");
    }

    [HttpPatch("replicates/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            var updatedStatus = await replicateService.UpdateReviewStatus(id, request);
            return Ok(updatedStatus);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { detail = "Replicate record not found" });
        }
    }
}