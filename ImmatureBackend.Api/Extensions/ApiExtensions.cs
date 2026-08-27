using System.Diagnostics;

namespace ImmatureBackend.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddOpenApi();

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToHexString();
            };
        });

        return services;
    }
}