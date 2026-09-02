using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

public sealed record ReplicateListItem
{
    public string Id { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string SampleId { get; set; } = string.Empty;
    public List<GrainBox> AiPredictedGrains { get; set; } = new();
    public List<GrainBox> ConfirmedGrains { get; set; } = new();
    public decimal ImmatureWeight { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public ReviewStatus ReviewStatus { get; set; } 
}