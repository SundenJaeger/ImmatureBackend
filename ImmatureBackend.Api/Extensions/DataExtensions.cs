using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Infrastructure.Configurations;
using ImmatureBackend.Infrastructure.Persistence;
using ImmatureBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ImmatureBackend.Api.Extensions;

public static class DataExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDataServices(IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddScoped<IReplicateRepository, ReplicateRepository>();

            return services;
        }

        private IServiceCollection AddDatabase(IConfiguration configuration)
        {
            services
                .AddOptions<DatabaseSettings>()
                .Bind(configuration.GetSection(DatabaseSettings.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddDbContext<AppDbContext>((provider, options) =>
            {
                var settings = provider
                    .GetRequiredService<IOptions<DatabaseSettings>>()
                    .Value;

                options.UseNpgsql(settings.ConnectionString);
            });

            return services;
        }
    }
}