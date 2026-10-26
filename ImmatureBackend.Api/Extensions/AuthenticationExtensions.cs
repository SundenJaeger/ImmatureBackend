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

            return services;
        }
    }
}