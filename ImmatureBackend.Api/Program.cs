using ImmatureBackend.Api.Extensions;

namespace ImmatureBackend.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = builder.Configuration;
        config.AddEnvironmentVariables();

        builder.Services
            .AddApplicationServices()
            .AddDataServices(builder.Configuration)
            .AddApiAuthentication()
            .AddApiServices()
            .AddValidators()
            .AddExceptionHandlers();

        var app = builder.Build();

        app.UseApiPipeline();

        app.Run();
    }
}