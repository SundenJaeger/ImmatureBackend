using System.ComponentModel.DataAnnotations;

namespace ImmatureBackend.Infrastructure.Configurations;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    [Required] public string Cloudname { get; set; } = string.Empty;
    [Required] public string ApiKey { get; set; } = string.Empty;
    [Required] public string ApiSecret { get; set; } = string.Empty;
}