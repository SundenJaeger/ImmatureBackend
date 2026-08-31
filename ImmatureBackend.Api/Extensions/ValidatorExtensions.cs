using FluentValidation;
using ImmatureBackend.Api.Validation;
using ImmatureBackend.Application.Validators;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace ImmatureBackend.Api.Extensions;

public static class ValidatorExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(option =>
        {
            option.DisableBuiltInModelValidation = true;
            option.OverrideDefaultResultFactoryWith<ValidationResultFactory>();
        });

        services.AddValidatorsFromAssemblyContaining<UpdateStatusRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<PredictRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<ReplicateRequestValidator>();

        return services;
    }
}