using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Utils;
using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ImmatureBackend.Domain.Models;

[Table("replicates")]
public class ReplicateEntity : BaseModel
{
    [PrimaryKey("id", false)] public Guid Id { get; set; } = Guid.NewGuid();

    [Column("technician_name")] public string TechnicianName { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("sample_id")] public string SampleId { get; set; } = string.Empty;

    [Column("ai_predicted_grains")] public string AiPredictedGrains { get; set; } = string.Empty;

    [Column("confirmed_grains")] public string ConfirmedGrains { get; set; } = string.Empty;

    [Column("immature_weight")] public decimal ImmatureWeight { get; set; }

    [Column("percentage")] public decimal Percentage { get; set; }

    [Column("grade")] public string Grade { get; set; } = string.Empty;

    [Column("original_image")]
    [JsonConverter(typeof(ByteaConverter))]
    public byte[]? OriginalImage { get; set; }

    [Column("review_status")] public ReviewStatus ReviewStatus { get; set; } = ReviewStatus.Review;
}