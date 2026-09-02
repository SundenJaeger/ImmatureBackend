using ImmatureBackend.Infrastructure.Configurations.Supabase;
using Microsoft.Extensions.Options;
using Supabase;

namespace ImmatureBackend.Api.Extensions;

public static class SupabaseExtensions
{
    public static IServiceCollection AddSupabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SupabaseSettings>()
            .Bind(configuration.GetSection(SupabaseSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<Client>(options =>
        {
            var settings = options
                .GetRequiredService<IOptions<SupabaseSettings>>()
                .Value;

            return new Client(
                settings.Url,
                settings.Key,
                new SupabaseOptions
                {
                    AutoConnectRealtime = true
                }
            );
        });
        services.AddHostedService<SupabaseInitializer>();

        return services;
    }
}