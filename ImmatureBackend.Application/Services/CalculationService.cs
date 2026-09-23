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

        return Math.Round(weight / 30m * 100m, 2);
    }

    public Grade AssignGrade(decimal percentage)
    {
        return percentage switch
        {
            < 2.0m => Grade.Pr,
            <= 5.0m => Grade.G1,
            <= 10.0m => Grade.G2,
            <= 15.0m => Grade.G3,
            _ => Grade.BelowStandard
        };
    }
}