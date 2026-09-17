using Serilog;

namespace ImmatureBackend.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseCookiePolicy();

        app.UseCors("immature-dashboard-frontend");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}