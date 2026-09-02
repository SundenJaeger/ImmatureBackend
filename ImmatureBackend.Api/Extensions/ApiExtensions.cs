using System.Diagnostics;

namespace ImmatureBackend.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddCors(options =>
        {
            options.AddPolicy("immature-dashboard-frontend", policy =>
            {
                policy
                    .WithOrigins("https://immature-dashboard.netlify.app", "http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

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