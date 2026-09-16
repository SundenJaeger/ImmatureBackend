using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;

namespace ImmatureBackend.Api.Validation;

public sealed class ValidationResultFactory(ILogger<ValidationResultFactory> logger)
    : IFluentValidationAutoValidationResultFactory
{
    public Task<IActionResult?> CreateActionResult(ActionExecutingContext context,
        ValidationProblemDetails validationProblemDetails,
        IDictionary<IValidationContext, ValidationResult> validationResults)
    {
        logger.LogWarning(
            "Request validation failed. EndPoint: {Endpoint}, Errors: {Errors}",
            context.ActionDescriptor.DisplayName,
            JsonConvert.SerializeObject(validationProblemDetails.Errors)
        );

        return Task.FromResult<IActionResult?>(new BadRequestObjectResult(validationProblemDetails));
    }
}