using CloudinaryDotNet;
using ImmatureBackend.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace ImmatureBackend.Api.Extensions;

public static class CloudinaryExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCloudinary(IConfiguration configuration)
        {
            services
                .AddOptions<CloudinaryOptions>()
                .Bind(configuration.GetSection(CloudinaryOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(provider =>
            {
                var options = provider
                    .GetRequiredService<IOptions<CloudinaryOptions>>()
                    .Value;

                var account = new Account(
                    options.Cloudname,
                    options.ApiKey,
                    options.ApiSecret
                );

                return new Cloudinary(account)
                {
                    Api = { Secure = true }
                };
            });

            return services;
        }
    }
}