using ImmatureBackend.Enums;

namespace ImmatureBackend.Models;

public class ReplicateEntity
{
    public Guid Id { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string SampleId { get; set; } = string.Empty;
    public string AiPredictedGrains { get; set; } = string.Empty;
    public string ConfirmedGrains { get; set; } = string.Empty;
    public decimal ImmatureWeight { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public byte[]? OriginalImage { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
}