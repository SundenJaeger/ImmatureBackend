using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Handlers;

public sealed class ValidationExceptionHandler(
    ILogger<ValidationExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Exception Occured: {Message}", exception.Message);
        
        if (exception is not ValidationException ex)
        {
            return false;
        }

        var errors = new ValidationResult(ex.Errors)
            .ToDictionary();

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest
        };

        context.Response.StatusCode = 400;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem
            });

        return true;
    }
}