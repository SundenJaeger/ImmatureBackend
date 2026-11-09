using System.Diagnostics;
using System.Reflection;
using Microsoft.OpenApi;
using ZiggyCreatures.Caching.Fusion;

namespace ImmatureBackend.Api.Extensions;

public static class ApiExtensions
{
    private const string DashboardOrigin = "https://immature-dashboard.netlify.app";
    private const string LocalDashboardOrigin = "http://localhost:5173";

    public static IServiceCollection AddApiServices(this IServiceCollection services, IHostEnvironment environment)
    {
        services.AddControllers()
            .AddNewtonsoftJson();

        services.AddCors(options =>
        {
            options.AddPolicy("immature-dashboard-frontend", policy =>
            {
                var origins = environment.IsDevelopment()
                    ? new[] { DashboardOrigin, LocalDashboardOrigin }
                    : new[] { DashboardOrigin };

                policy
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition");
            });
        });

        services
            .AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(5)
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Immature Backend API",
                Version = "v1",
                Description = "Grain detection and replicate review API used by the Android capture app " +
                              "and the review dashboard. Every endpoint requires a Supabase access token " +
                              "in the Authorization header (Bearer <token>). Capture endpoints accept any " +
                              "signed-in user; dashboard endpoints require the 'dashboard' app role."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Supabase access token. Paste the token only, without the 'Bearer ' prefix."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

            var apiXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, apiXml));

            const string applicationXml = "ImmatureBackend.Application.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, applicationXml));
        });

        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.Remove("traceId");
            };
        });

        return services;
    }
}