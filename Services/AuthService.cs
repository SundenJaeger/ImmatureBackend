using ImmatureBackend.Services.Interfaces;

namespace ImmatureBackend.Services;

public class AuthService(IConfiguration configuration) : IAuthService
{
    private readonly string _apiKey = configuration["EXPECTED_API_KEY"] ??
                                      throw new InvalidOperationException("EXPECTED_API_KEY is not set");

    public bool IsValidKey(string key)
    {
        return !string.IsNullOrEmpty(key) && key == _apiKey;
    }
}