using System.Security.Claims;
using AspNetCore.Authentication.ApiKey;

namespace ImmatureBackend.Api.Authentication;

public class ApiKey(
    string key,
    string ownerName,
    IReadOnlyCollection<Claim>? claims = null)
    : IApiKey
{
    public string Key { get; } = key;
    public string OwnerName { get; } = ownerName;
    public IReadOnlyCollection<Claim> Claims { get; } = claims ?? [];
}