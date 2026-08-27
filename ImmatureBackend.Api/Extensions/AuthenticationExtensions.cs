using AspNetCore.Authentication.ApiKey;
using ImmatureBackend.Api.Authentication;

namespace ImmatureBackend.Api.Extensions;

public static class AuthenticationExtensions
{
    private const string ApiRealm = "Immature Backend API";
    private const string ApiKeyName = "X-API-Key";
    
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddScoped<IApiKeyProvider, ApiKeyProvider>();

        services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader<ApiKeyProvider>(options =>
            {
                options.Realm = ApiRealm;
                options.KeyName = ApiKeyName;
            });
        services.AddAuthorization();

        return services;
    }
}