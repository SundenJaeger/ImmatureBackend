using FluentResults;
using ImmatureBackend.Application.Errors;
using ImmatureBackend.Application.Responses;
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
            var body = new ErrorResponse { Message = error.Message };

            return error switch
            {
                ImageNotFoundError or ReplicateNotFoundError => new NotFoundObjectResult(body),
                InvalidImageError => new UnprocessableEntityObjectResult(body),
                _ => new BadRequestObjectResult(body)
            };
        }
    }
}