using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Services;

namespace ImmatureBackend.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IGrainDetector, PlaceholderDetector>();
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<IReplicateService, ReplicateService>();

        return services;
    }
}