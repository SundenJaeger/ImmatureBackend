using AspNetCore.Authentication.ApiKey;

namespace ImmatureBackend.Api.Authentication;

public class ApiKeyProvider(IConfiguration configuration) : IApiKeyProvider
{
    public Task<IApiKey?> ProvideAsync(string key)
    {
        var configuredKey = configuration["API_KEY"];

        if (string.IsNullOrWhiteSpace(configuredKey) || key != configuredKey)
        {
            return Task.FromResult<IApiKey?>(null);
        }

        IApiKey apiKey = new ApiKey(key, "Immature Backend");

        return Task.FromResult<IApiKey?>(apiKey);
    }
}