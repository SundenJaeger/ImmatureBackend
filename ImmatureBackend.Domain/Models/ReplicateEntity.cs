using ImmatureBackend.Domain.Enums;
using ImmatureBackend.Domain.Utils;
using Newtonsoft.Json;

namespace ImmatureBackend.Domain.Models;

public class ReplicateEntity
{
    public Guid Id { get; set; }
    public required string TechnicianName { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string SampleId { get; set; }
    public required string AiPredictedGrains { get; set; }
    public required string ConfirmedGrains { get; set; }
    public decimal ImmatureWeight { get; set; }
    public decimal Percentage { get; set; }

    public required string Grade { get; set; }

    [JsonConverter(typeof(ByteaConverter))]
    public byte[]? OriginalImage { get; set; }

    public ReviewStatus ReviewStatus { get; set; }
}