using System.Diagnostics;
using System.Reflection;
using Microsoft.OpenApi;

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
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Immature Backend API",
                Version = "v1",
                Description = "Grain detection and replicate review API used by the Android capture app " +
                              "and the review dashboard. Every endpoint requires an X-API-Key header."
            });

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "The shared key issued to the Android and Dashboard clients."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
            });

            options.IncludeXmlComments(Assembly.GetExecutingAssembly());
        });

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