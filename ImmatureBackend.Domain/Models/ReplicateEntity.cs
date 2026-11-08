using ImmatureBackend.Domain.Enums;
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
    public Grade Grade { get; set; }
    public required string ImagePublicId { get; set; }
    public required string ImageSecureUrl { get; set; }
    public ReviewStatus ReviewStatus { get; set; }
    public required string Season { get; set; }
    public required string IndexNumber { get; set; }
    public required string SelectionName { get; set; }
    public int SampleNo { get; set; }
    public DateTime CapturedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool MeetsRecommendedValue { get; set; }
}