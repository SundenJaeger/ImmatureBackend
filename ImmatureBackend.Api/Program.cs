using ImmatureBackend.Api.Extensions;

namespace ImmatureBackend.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddSentry();
        builder.AddSerilog();

        var config = builder.Configuration;
        config.AddEnvironmentVariables();

        builder.Services
            .AddApplicationServices()
            .AddDataServices(builder.Configuration)
            .AddSupabaseAuthentication(builder.Configuration)
            .AddApiServices(builder.Environment)
            .AddCloudinary(builder.Configuration)
            .AddValidators()
            .AddExceptionHandlers()
            .AddHealthChecks();

        var app = builder.Build();

        app.MapHealthChecks("health");

        app.UseApiPipeline();

        app.Run();
    }
}