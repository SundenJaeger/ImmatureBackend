namespace ImmatureBackend.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseCookiePolicy();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}