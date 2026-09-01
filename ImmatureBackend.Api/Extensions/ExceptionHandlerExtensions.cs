using ImmatureBackend.Api.Handlers;

namespace ImmatureBackend.Api.Extensions;

public static class ExceptionHandlerExtensions
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }
}