using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ImmatureBackend.IntegrationTest.Infrastructure;

public static class TestJwt
{
    public const string SupabaseUrl = "https://test.supabase.local";

    public static readonly SymmetricSecurityKey SigningKey =
        new("integration-tests-only-signing-key-0123456789"u8.ToArray());

    public static string Authenticated() => Create(appMetadata: new { provider = "email" });
    public static string Dashboard() => Create(appMetadata: new { provider = "email", app_role = "dashboard" });
    public static string Anonymous() => Create(appMetadata: new { provider = "anonymous" }, isAnonymous: true);

    public static string Expired() => Create(appMetadata: new { provider = "email" }, lifetime: TimeSpan.FromHours(-2));

    private static string Create(object appMetadata, bool isAnonymous = false, TimeSpan? lifetime = null)
    {
        var payload = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["iss"] = $"{SupabaseUrl}/auth/v1",
            ["aud"] = "authenticated",
            ["sub"] = Guid.NewGuid().ToString(),
            ["exp"] = DateTimeOffset.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)).ToUnixTimeSeconds(),
            ["is_anonymous"] = isAnonymous,
            ["app_metadata"] = appMetadata
        });

        return new JsonWebTokenHandler().CreateToken(
            payload,
            new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256));
    }
}