namespace ImmatureBackend.Api.Extensions;

public static class SentryExtensions
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
    }
}