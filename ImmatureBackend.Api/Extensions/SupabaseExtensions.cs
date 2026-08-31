using Supabase;

namespace ImmatureBackend.Api.Extensions;

public static class SupabaseExtensions
{
    public static IServiceCollection AddSupabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var supabaseUrl = configuration["SUPABASE_URL"]
                          ?? throw new InvalidOperationException("SUPABASE_URL is not set");

        var supabaseKey = configuration["SUPABASE_KEY"]
                          ?? throw new InvalidOperationException("SUPABASE_KEY is not set");

        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        services.AddSingleton<Client>(_ => new Client(supabaseUrl, supabaseKey, options));
        services.AddHostedService<SupabaseInitializer>();

        return services;
    }
}