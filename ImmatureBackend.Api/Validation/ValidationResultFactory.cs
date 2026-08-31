using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace ImmatureBackend.Api.Validation;

public sealed class ValidationResultFactory(ILogger<ValidationResultFactory> logger)
    : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(ActionExecutingContext context,
        ValidationProblemDetails validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        var traceId = Activity.Current?.TraceId.ToHexString() ?? context.HttpContext.TraceIdentifier;

        logger.LogWarning(
            "Request validation failed. TraceId: {TraceId}, EndPoint: {Endpoint}, Errors: {@Errors}",
            traceId,
            context.ActionDescriptor.DisplayName,
            validationProblemDetails.Errors
        );

        return Task.FromResult<IActionResult?>(new BadRequestObjectResult(validationProblemDetails));
    }
}