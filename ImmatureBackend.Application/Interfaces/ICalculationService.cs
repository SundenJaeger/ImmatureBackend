using ImmatureBackend.Application.Responses;
using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Interfaces;

public interface ICalculationService
{
    decimal CalculatePercentage(decimal weight);
    Grade AssignGrade(decimal percentage);
    bool MeetsRecommendedValue(decimal percentage);
    decimal CalculateProcessingSeconds(DateTimeOffset capturedAt, DateTimeOffset submittedAt);
    decimal? CalculateConfidence(IReadOnlyCollection<GrainBox> grains);
    ReplicateQuality AssessQuality(decimal? confidence, bool manuallyOverridden);
    decimal CalculateAverageWeight(IReadOnlyCollection<decimal> weights);
}