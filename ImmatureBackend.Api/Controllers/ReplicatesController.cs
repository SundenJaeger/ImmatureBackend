using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReplicatesController(
    IReplicateService replicateService) : ControllerBase
{
    [HttpPost("replicate")]
    public async Task<IActionResult> Replicate([FromForm] ReplicateRequest model)
    {
        var result = await replicateService.CreateAsync(model);

        return Ok(result);
    }

    [HttpGet("replicates")]
    public async Task<IActionResult> GetAll()
    {
        var entities = await replicateService.GetAllReplicateListItemsAsync();

        return Ok(entities);
    }

    [HttpGet("images/{id}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var (imageBytes, contentType) = await replicateService.GetImage(id);

        return File(imageBytes, contentType);
    }

    [HttpPatch("replicates/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var updatedStatus = await replicateService.UpdateReviewStatus(id, request);

        return Ok(updatedStatus);
    }
}