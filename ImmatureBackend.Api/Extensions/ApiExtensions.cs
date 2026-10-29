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