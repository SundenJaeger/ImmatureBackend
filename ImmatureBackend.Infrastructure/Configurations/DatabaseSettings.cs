using System.ComponentModel.DataAnnotations;

namespace ImmatureBackend.Infrastructure.Configurations;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    [Required]
    public required string ConnectionString { get; init; } = string.Empty;
}