using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Responses;

/// <summary>A single row in the GET /api/replicates response.</summary>
public sealed record ReplicateListItem
{
    public string Id { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string SampleId { get; set; } = string.Empty;
    public List<GrainBox> AiPredictedGrains { get; set; } = new();
    public List<GrainBox> ConfirmedGrains { get; set; } = new();
    public decimal ImmatureWeight { get; set; }

    /// <summary>Computed as (weight / 30) * 100.</summary>
    public decimal Percentage { get; set; }

    /// <summary>One of "Pr", "G1", "G2", "G3", "Below Standard".</summary>
    public string Grade { get; set; } = string.Empty;

    public ReviewStatus ReviewStatus { get; set; }
}