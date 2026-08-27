using ImmatureBackend.Data.Interfaces;
using ImmatureBackend.Data.Repositories;

namespace ImmatureBackend.Api.Extensions;

public static class DataExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services)
    {
        services.AddScoped<IReplicateRepository, ReplicateRepository>();

        return services;
    }
}