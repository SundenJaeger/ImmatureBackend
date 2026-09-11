using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class PredictController(
    IGrainDetector grainDetector) : ControllerBase
{
    /// <summary>
    /// Detects grain boxes in a 1024x1024 sample image. Prediction-only, nothing is saved.
    /// </summary>
    /// <remarks>
    /// Multipart form-data: image + technicianName + sampleId. Only boxes with confidence &gt;= 0.5
    /// are returned; an empty "grains" array is valid and expected sometimes.
    /// </remarks>
    /// <response code="200">Detected grain boxes for the image.</response>
    /// <response code="400">A required field is missing, or the image is empty/over 10 MB.</response>
    /// <response code="401">Missing or invalid X-API-Key header.</response>
    [ProducesResponseType(typeof(PredictResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromForm] PredictRequest request)
    {
        using var ms = new MemoryStream();
        await request.Image.CopyToAsync(ms);
        
        var allGrains = grainDetector.Detect(ms.ToArray());

        return Ok(allGrains);
    }
}