using System.Diagnostics;
using ImmatureBackend.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ImmatureBackend.Api.Handlers;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.TraceId.ToHexString() ?? httpContext.TraceIdentifier;
        var endpoint = httpContext.GetEndpoint()?.DisplayName ?? httpContext.Request.Path;

        var (statusCode, detail) = exception switch
        {
            ImageNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            ReplicateNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occured.")
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception,
                "Unhandled server exception. TraceId: {TraceId}, Endpoint: {Endpoint}, " +
                "Method: {Method}, Path: {Path}, ExceptionType: {ExceptionType}, " +
                "StatusCode: {StatusCode}, Message: {Message}",
                traceId,
                endpoint,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.GetType().Name,
                statusCode,
                exception.Message
            );
        }
        else
        {
            logger.LogWarning(
                "Request failed. TraceId: {TraceId}, Endpoint: {Endpoint}, " +
                "Method: {Method}, Path: {Path}, ExceptionType: {ExceptionType}, " +
                "StatusCode: {StatusCode}, Message: {Message}",
                traceId,
                endpoint,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.GetType().Name,
                statusCode,
                exception.Message
            );
        }

        httpContext.Response.StatusCode = statusCode;

        await problemDetailsService.WriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Detail = detail
                },
                Exception = exception
            }
        );

        return true;
    }
}