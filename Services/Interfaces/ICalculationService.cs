namespace ImmatureBackend.Services.Interfaces;

public interface ICalculationService
{
    decimal CalculatePercentage(decimal weight);
    string AssignGrade(decimal percentage);
}