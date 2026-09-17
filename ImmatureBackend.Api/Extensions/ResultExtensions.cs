using FluentResults;
using ImmatureBackend.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Extensions;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public IActionResult ToActionResult()
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Value);
            }

            var error = result.Errors.First();

            return error switch
            {
                ImageNotFoundError or ReplicateNotFoundError => new NotFoundObjectResult(
                    new { message = error.Message }),
                InvalidImageError => new UnprocessableEntityObjectResult(new { message = error.Message }),
                _ => new BadRequestObjectResult(new { message = error.Message })
            };
        }
    }
}