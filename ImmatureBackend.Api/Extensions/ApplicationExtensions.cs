using FileSignatures;
using FileSignatures.Formats;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Application.Services;

namespace ImmatureBackend.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IGrainDetector>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var modelPath = config["GrainDetector:ModelPath"] ??
                            Path.Combine(AppContext.BaseDirectory, "Models", "granular_efficientnet_b0.onnx");

            return new OnnxGrainDetector(modelPath, provider.GetRequiredService<ILogger<OnnxGrainDetector>>());
        });
        services.AddScoped<ICalculationService, CalculationService>();
        services.AddScoped<IReplicateService, ReplicateService>();
        services.AddScoped<IImageService, CloudinaryImageService>();

        services.AddSingleton<IFileFormatInspector>(_ =>
        {
            var formats = new List<FileFormat>
            {
                new Png(),
                new Jpeg()
            };

            return new FileFormatInspector(formats);
        });

        return services;
    }
}