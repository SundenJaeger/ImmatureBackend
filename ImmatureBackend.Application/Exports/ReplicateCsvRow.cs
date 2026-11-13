using CsvHelper;

namespace ImmatureBackend.Application.Exports;

[CsvRecord]
public partial class ReplicateCsvRow
{
    [CsvColumn("id", Index = 0)]
    public string Id { get; set; } = string.Empty;

    [CsvColumn("technician_name", Index = 1)]
    public string TechnicianName { get; set; } = string.Empty;

    [CsvColumn("season", Index = 2)]
    public string Season { get; set; } = string.Empty;

    [CsvColumn("index_number", Index = 3)]
    public string IndexNumber { get; set; } = string.Empty;

    [CsvColumn("selection_name", Index = 4)]
    public string SelectionName { get; set; } = string.Empty;

    [CsvColumn("sample_id", Index = 5)]
    public string SampleId { get; set; } = string.Empty;

    [CsvColumn("sample_no", Index = 6)]
    public int? SampleNo { get; set; }

    [CsvColumn("immature_weight", Index = 7)]
    public decimal ImmatureWeight { get; set; }

    [CsvColumn("percentage", Index = 8)]
    public decimal Percentage { get; set; }

    [CsvColumn("grade", Index = 9)]
    public string Grade { get; set; } = string.Empty;

    [CsvColumn("meets_recommended_value", Index = 10)]
    public bool MeetsRecommendedValue { get; set; }

    [CsvColumn("review_status", Index = 11)]
    public string ReviewStatus { get; set; } = string.Empty;

    [CsvColumn("captured_at", Index = 12)]
    public string CapturedAt { get; set; } = string.Empty;

    [CsvColumn("submitted_at", Index = 13)]
    public string SubmittedAt { get; set; } = string.Empty;

    [CsvColumn("processing_seconds", Index = 14)]
    public decimal ProcessingSeconds { get; set; }

    [CsvColumn("created_at", Index = 15)]
    public string CreatedAt { get; set; } = string.Empty;

    [CsvColumn("image_secure_url", Index = 16)]
    public string ImageSecureUrl { get; set; } = string.Empty;

    [CsvColumn("ai_predicted_grains", Index = 17)]
    public string AiPredictedGrains { get; set; } = string.Empty;

    [CsvColumn("confirmed_grains", Index = 18)]
    public string ConfirmedGrains { get; set; } = string.Empty;

    [CsvColumn("replicate_no", Index = 19)]
    public int ReplicateNo { get; set; }

    [CsvColumn("replicate_label", Index = 20)]
    public string ReplicateLabel { get; set; } = string.Empty;

    [CsvColumn("confidence", Index = 21)]
    public decimal? Confidence { get; set; }

    [CsvColumn("quality", Index = 22)]
    public string Quality { get; set; } = string.Empty;

    [CsvColumn("manually_overridden", Index = 23)]
    public bool ManuallyOverridden { get; set; }
}