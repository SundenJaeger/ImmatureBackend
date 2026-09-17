using ImmatureBackend.Api.Extensions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ReplicatesController(
    IReplicateService replicateService) : ControllerBase
{
    /// <summary>
    /// Submits a confirmed replicate: image, grain boxes, and weight. Computes percentage/grade and saves the row.
    /// </summary>
    /// <remarks>
    /// Multipart form-data. "aiPredictedGrains" and "confirmedGrains" are JSON-encoded strings
    /// (a JSON array serialized into a single form field), not native JSON arrays.
    /// </remarks>
    /// <response code="200">Replicate saved; returns its id, computed percentage, and grade.</response>
    /// <response code="400">A required field is missing, or weight is not greater than zero.</response>
    /// <response code="401">Missing or invalid X-API-Key header.</response>
    /// <response code="422">The uploaded file isn't a JPEG or PNG image.</response>
    [ProducesResponseType(typeof(ReplicateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [HttpPost("replicate")]
    public async Task<IActionResult> Replicate([FromForm] ReplicateRequest model)
    {
        var result = await replicateService.CreateAsync(model);

        if (result.IsFailed)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists every replicate row for the dashboard. Excludes the raw image bytes fetch those separately.
    /// </summary>
    /// <response code="200">All replicate rows.</response>
    /// <response code="401">Missing or invalid X-API-Key header.</response>
    [ProducesResponseType(typeof(IReadOnlyList<ReplicateListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("replicates")]
    public async Task<IActionResult> GetAll()
    {
        var entities = await replicateService.GetAllReplicateListItemsAsync();

        return Ok(entities);
    }

    /// <summary>
    /// Returns the original JPEG/PNG image bytes for a replicate.
    /// </summary>
    /// <param name="id">The replicate's id.</param>
    /// <response code="200">Raw image bytes.</response>
    /// <response code="401">Missing or invalid X-API-Key header.</response>
    /// <response code="404">No replicate (or no stored image) with that id.</response>
    /// <response code="422">The stored bytes aren't a JPEG or PNG image.</response>
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/jpeg", "image/png")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [HttpGet("images/{id}")]
    public async Task<IActionResult> GetImage(Guid id)
    {
        var result = await replicateService.GetImage(id);

        if (result.IsFailed)
        {
            return result.ToActionResult();
        }

        var (imageBytes, contentType) = result.Value;

        return File(imageBytes, contentType);
    }

    /// <summary>
    /// Updates a replicate's review status (e.g. after the dashboard user accepts or rejects it).
    /// </summary>
    /// <param name="id">The replicate's id.</param>
    /// <param name="request">The new status: "review", "accepted", "rejected", or "retraining".</param>
    /// <response code="200">Updated id and status.</response>
    /// <response code="400">Status is missing or not one of the valid values.</response>
    /// <response code="401">Missing or invalid X-API-Key header.</response>
    /// <response code="404">No replicate with that id.</response>
    [ProducesResponseType(typeof(UpdateStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPatch("replicates/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        var result = await replicateService.UpdateReviewStatus(id, request);

        if (result.IsFailed)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value);
    }
}