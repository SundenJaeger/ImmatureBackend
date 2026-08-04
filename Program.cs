using ImmatureBackend.Repositories;
using ImmatureBackend.Repositories.Interfaces;
using ImmatureBackend.Services;
using ImmatureBackend.Services.Interfaces;
using Supabase;

namespace ImmatureBackend;

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

        builder.Services.AddSingleton(new Client(supabaseUrl, supabaseKey, supabaseOptions));
        builder.Services.AddScoped<IGrainDetector, PlaceholderDetector>();
        builder.Services.AddScoped<ICalculationService, CalculationService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IReplicateRepository, ReplicateRepository>();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddOpenApi();
        builder.Services.AddProblemDetails();

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