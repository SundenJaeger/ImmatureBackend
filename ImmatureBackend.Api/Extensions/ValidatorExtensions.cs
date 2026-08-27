using FluentValidation;
using ImmatureBackend.Application.Validators;

namespace ImmatureBackend.Api.Extensions;

public static class ValidatorExtensions
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<UpdateStatusRequestValidator>();

        return services;
    }
}