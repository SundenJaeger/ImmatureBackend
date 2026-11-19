using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(Policy = "Capture")]
public class GradeController(ICalculationService calculationService) : ControllerBase
{
    /// <summary>
    /// Returns the immature percentage and grade for a weight. Preview only, nothing is saved.
    /// </summary>
    /// <remarks>
    /// Uses the same calculation as POST /api/replicate, so the grade matches what a submission with the
    /// same weight would store. "weight" is in grams, greater than 0 and at most 30, written with a dot.
    /// </remarks>
    /// <response code="200">Weight, percentage (1 decimal), grade and meetsRecommendedValue.</response>
    /// <response code="400">Weight is missing, not a number, or outside 0 to 30 g.</response>
    /// <response code="401">Missing, expired or invalid bearer token.</response>
    [ProducesResponseType(typeof(GradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("grade")]
    public IActionResult Grade([FromQuery] GradeRequest request)
    {
        var weight = request.Weight!.Value;
        var percentage = calculationService.CalculatePercentage(weight);

        return Ok(new GradeResponse
        {
            Weight = weight,
            Percentage = percentage,
            Grade = calculationService.AssignGrade(percentage),
            MeetsRecommendedValue = calculationService.MeetsRecommendedValue(percentage)
        });
    }
}
