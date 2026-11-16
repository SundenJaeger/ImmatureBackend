using ImmatureBackend.Api;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace ImmatureBackend.IntegrationTest.Infrastructure;

public sealed class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    private ApiFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Database__ConnectionString", _postgres.GetConnectionString());
        Environment.SetEnvironmentVariable("Supabase__Url", TestJwt.SupabaseUrl);
        Environment.SetEnvironmentVariable("Cloudinary__Cloudname", "test");
        Environment.SetEnvironmentVariable("Cloudinary__ApiKey", "test");
        Environment.SetEnvironmentVariable("Cloudinary__ApiSecret", "test");
        Environment.SetEnvironmentVariable("Sentry__Dsn", "");

        _factory = new ApiFactory();

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    public HttpClient CreateClient(string? bearerToken = null)
    {
        var client = _factory.CreateClient();

        if (bearerToken is not null)
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return client;
    }

    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.ConfigurationManager = null;
                    options.TokenValidationParameters.IssuerSigningKey = TestJwt.SigningKey;
                });

                services.RemoveAll<IImageService>();
                services.AddScoped<IImageService, FakeImageService>();

                services.RemoveAll<IGrainDetector>();
                services.AddScoped<IGrainDetector, FakeGrainDetector>();
            });
        }
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}