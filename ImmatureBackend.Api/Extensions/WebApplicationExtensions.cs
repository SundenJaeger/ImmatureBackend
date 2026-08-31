namespace ImmatureBackend.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseHttpsRedirection();
        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}