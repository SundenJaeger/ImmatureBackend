using ImmatureBackend.Api.Extensions;

namespace ImmatureBackend.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = builder.Configuration;
        config.AddEnvironmentVariables();

        builder.Services
            .AddSupabase(builder.Configuration)
            .AddApplicationServices()
            .AddDataServices()
            .AddApiAuthentication()
            .AddApiServices()
            .AddValidators()

        var app = builder.Build();

        app.UseApiPipeline();
        
        await app.RunAsync();
    }
}