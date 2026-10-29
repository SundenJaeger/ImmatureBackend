using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ImmatureBackend.Api.Extensions;

public static class AuthenticationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSupabaseAuthentication(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"]!
                .TrimEnd();

            var issuer = $"{url}/auth/v1";

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = issuer;
                    options.Audience = "authenticated";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = "authenticated",
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy("Capture", policy => policy.RequireAuthenticatedUser());

                    options.AddPolicy("Dashboard", policy => policy
                        .RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var anon = context.User.FindFirst("is_anonymous")?.Value;
                            if (string.Equals(anon, "true", StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }

                            var metadata = context.User.FindFirst("app_metadata")?.Value;
                            if (string.IsNullOrEmpty(metadata))
                            {
                                return false;
                            }

                            using var doc = JsonDocument.Parse(metadata);

                            return doc.RootElement.TryGetProperty("app_role", out var role) &&
                                   role.GetString() == "dashboard";
                        }));
                });

            return services;
        }
    }
}