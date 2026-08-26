using ImmatureBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ImmatureBackend.Api.Filters;

public class ApiKeyFilter(IAuthService authService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.Any(em => em is AllowAnonymousAttribute))
            return;
        
        var headers = context.HttpContext.Request.Headers;
        if (!headers.TryGetValue("X-API-Key", out var apiKey) || string.IsNullOrEmpty(apiKey))
        {
            context.Result = new UnauthorizedObjectResult(new { detail = "Missing API Key" });
            return;
        }

        if (!authService.IsValidKey(apiKey!))
        {
            context.Result = new UnauthorizedObjectResult(new { detail = "Invalid API Key" });
        }
    }
}