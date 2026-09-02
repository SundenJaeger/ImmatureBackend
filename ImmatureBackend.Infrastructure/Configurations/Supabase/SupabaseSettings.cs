using System.ComponentModel.DataAnnotations;

namespace ImmatureBackend.Infrastructure.Configurations.Supabase;

public sealed class SupabaseSettings
{
    public const string SectionName = "Supabase";

    [Required] 
    [Url] 
    public required string Url { get; init; } = string.Empty;

    [Required] 
    public required string Key { get; init; } = string.Empty;
}