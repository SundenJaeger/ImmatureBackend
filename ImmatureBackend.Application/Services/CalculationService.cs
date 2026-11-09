using ImmatureBackend.Application.Constants;
using ImmatureBackend.Application.Interfaces;
using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Services;

public class CalculationService : ICalculationService
{
    public decimal CalculatePercentage(decimal weight)
    {
        if (weight <= 0)
        {
            return 0;
        }

        return Math.Round(weight / ReplicateRules.SampleWeightGrams * 100m, 1, MidpointRounding.AwayFromZero);
    }

    public Grade AssignGrade(decimal percentage)
    {
        return percentage switch
        {
            < ReplicateRules.RecommendedValuePercentage => Grade.Pr,
            <= 5.0m => Grade.G1,
            <= 10.0m => Grade.G2,
            <= 15.0m => Grade.G3,
            _ => Grade.NotClassified
        };
    }

    public bool MeetsRecommendedValue(decimal percentage)
    {
        return percentage < ReplicateRules.RecommendedValuePercentage;
    }

    public decimal CalculateProcessingSeconds(DateTimeOffset capturedAt, DateTimeOffset submittedAt)
    {
        var seconds = (decimal)(submittedAt - capturedAt).TotalSeconds;

        return Math.Round(seconds, 1, MidpointRounding.AwayFromZero);
    }
}