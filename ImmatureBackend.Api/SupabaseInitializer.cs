using Supabase;

namespace ImmatureBackend.Api;

public class SupabaseInitializer(Client client) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await client.InitializeAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}