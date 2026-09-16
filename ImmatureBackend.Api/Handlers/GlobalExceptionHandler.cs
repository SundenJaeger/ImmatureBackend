using ImmatureBackend.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Handlers;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, detail) = exception switch
        {
            ImageNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            ReplicateNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            InvalidImageException ex => (StatusCodes.Status422UnprocessableEntity, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occured.")
        };

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Detail = detail
                },
                Exception = exception
            }
        );

        return true;
    }
}