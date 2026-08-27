using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Requests;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Controllers;

[ApiController]
[Route("api")]
public class PredictController(
    IGrainDetector grainDetector) : ControllerBase
{
    [HttpPost("predict")]
    public async Task<IActionResult> Predict([FromForm] PredictRequest request)
    {
        using var ms = new MemoryStream();
        await request.Image.CopyToAsync(ms);
        
        var allGrains = grainDetector.Detect(ms.ToArray());

        return Ok(allGrains);
    }
}