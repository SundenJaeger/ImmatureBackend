using ImmatureBackend.Api.Extensions;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
public class ReplicatesController(
    IReplicateService replicateService) : ControllerBase
{
    /// <summary>
    /// Submits a confirmed replicate: image, grain boxes, and weight. Uploads the image to Cloudinary,
    /// computes percentage/grade and saves the row.
    /// </summary>
    /// <remarks>
    /// Multipart form-data. "aiPredictedGrains" and "confirmedGrains" are JSON-encoded strings
    /// (a JSON array serialized into a single form field), not native JSON arrays.
    /// </remarks>
    /// <response code="200">Replicate saved; returns its id, computed percentage, and grade.</response>
    /// <response code="400">A required field is missing, or weight is not greater than zero.</response>
    /// <response code="401">Missing, expired or invalid bearer token.</response>
    /// <response code="422">The uploaded file isn't a JPEG or PNG image, or the image upload failed.</response>
    [ProducesResponseType(typeof(ReplicateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [Authorize(Policy = "Capture")]
    [HttpPost("replicate")]
    public async Task<IActionResult> Replicate([FromForm] ReplicateRequest model, CancellationToken cancellationToken)
    {
        var result = await replicateService.CreateAsync(model, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists every replicate row for the dashboard, including each image's Cloudinary public id and URL.
    /// </summary>
    /// <response code="200">All replicate rows, each with "imagePublicId" and "imageSecureUrl".</response>
    /// <response code="401">Missing, expired or invalid bearer token.</response>
    /// <response code="403">The signed-in user doesn't have the dashboard role.</response>
    [ProducesResponseType(typeof(IReadOnlyList<ReplicateListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize(Policy = "Dashboard")]
    [HttpGet("replicates")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var entities = await replicateService.GetAllReplicateListItemsAsync(cancellationToken);

        return Ok(entities);
    }

    /// <summary>
    /// Updates a replicate's review status (e.g. after the dashboard user accepts or rejects it).
    /// </summary>
    /// <param name="id">The replicate's id.</param>
    /// <param name="request">The new status: "review", "accepted", "rejected", or "retraining".</param>
    /// <response code="200">Updated id and status.</response>
    /// <response code="400">Status is missing or not one of the valid values.</response>
    /// <response code="401">Missing, expired or invalid bearer token.</response>
    /// <response code="403">The signed-in user doesn't have the dashboard role.</response>
    /// <response code="404">No replicate with that id.</response>
    [ProducesResponseType(typeof(UpdateStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [Authorize(Policy = "Dashboard")]
    [HttpPatch("replicates/{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await replicateService.UpdateReviewStatus(id, request, cancellationToken);

        if (result.IsFailed)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value);
    }
}