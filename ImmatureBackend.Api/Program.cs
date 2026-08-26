using AspNetCore.Authentication.ApiKey;
using ImmatureBackend.Api.Authentication;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Services;
using ImmatureBackend.Data.Interfaces;
using ImmatureBackend.Data.Repositories;
using Supabase;

namespace ImmatureBackend.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = builder.Configuration;
        config.AddEnvironmentVariables();

        var supabaseUrl = config["SUPABASE_URL"] ?? throw new InvalidOperationException("SUPABASE_URL is not set");
        var supabaseKey = config["SUPABASE_KEY"] ?? throw new InvalidOperationException("SUPABASE_KEY is not set");

        var supabaseOptions = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        var supabaseClient = new Client(supabaseUrl, supabaseKey, supabaseOptions);
        await supabaseClient.InitializeAsync();

        builder.Services.AddSingleton(supabaseClient);
        builder.Services.AddScoped<IGrainDetector, PlaceholderDetector>();
        builder.Services.AddScoped<ICalculationService, CalculationService>();
        builder.Services.AddScoped<IReplicateRepository, ReplicateRepository>();
        builder.Services.AddScoped<IApiKeyProvider, ApiKeyProvider>();
        builder.Services.AddScoped<IReplicateService, ReplicateService>();
        
        builder.Services
            .AddAuthentication(ApiKeyDefaults.AuthenticationScheme)
            .AddApiKeyInHeader<ApiKeyProvider>(options =>
            {
                options.Realm = "Immature Backend API";
                options.KeyName = "X-API-KEY";
            });
        builder.Services.AddAuthorization();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.RunAsync();
    }
}