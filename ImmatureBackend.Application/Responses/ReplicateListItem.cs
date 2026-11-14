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

    /// <summary>Computed as (weight / 30) * 100, rounded to 1 decimal place. Rows saved before v2.0 keep 2 decimals.</summary>
    public decimal Percentage { get; set; }

    /// <summary>One of "Pr", "G1", "G2", "G3", "Not classified".</summary>
    public Grade Grade { get; set; }

    public ReviewStatus ReviewStatus { get; set; }

    public bool MeetsRecommendedValue { get; set; }

    public string? Season { get; set; }
    public string? IndexNumber { get; set; }
    public string? SelectionName { get; set; }

    public int? SampleNo { get; set; }

    /// <summary>1-based running number of this photo within its sample.</summary>
    public int ReplicateNo { get; set; }

    /// <summary>Sample id plus the replicate suffix, for example "S1-02".</summary>
    public string ReplicateLabel { get; set; } = string.Empty;

    /// <summary>Mean AI box confidence (0-1). Null when the AI returned no boxes with a confidence.</summary>
    public decimal? Confidence { get; set; }

    /// <summary>"good" when confident enough or manually overridden, otherwise "bad".</summary>
    public ReplicateQuality Quality { get; set; }

    public bool ManuallyOverridden { get; set; }

    /// <summary>When the photo was captured on the device (UTC).</summary>
    public DateTime? CapturedAt { get; set; }

    /// <summary>When the technician submitted the record (UTC).</summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>Cloudinary public id of the uploaded sample image.</summary>
    public string ImagePublicId { get; set; } = string.Empty;

    /// <summary>HTTPS URL of the sample image, loadable directly (no auth header needed).</summary>
    public string ImageSecureUrl { get; set; } = string.Empty;

    /// <summary>Seconds from capture to submission, 1 decimal.</summary>
    public decimal ProcessingSeconds { get; set; }
}