using ImmatureBackend.Services.Interfaces;

namespace ImmatureBackend.Services;

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

    public string AssignGrade(decimal percentage)
    {
        return percentage switch
        {
            < 2.0m => "Pr",
            <= 5.0m => "G1",
            <= 10.0m => "G2",
            <= 15.0m => "G3",
            _ => "Below Standard"
        };
    }
}