using Serilog;

namespace ImmatureBackend.Api.Extensions;

public static class LoggingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddSentry()
        {
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = builder.Configuration["Sentry:Dsn"];
                options.Debug = true;
                options.TracesSampleRate = 1.0;
                options.EnableLogs = true;
            });

            return builder;
        }

        public WebApplicationBuilder AddSerilog()
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services);
            });

            return builder;
        }
    }
}