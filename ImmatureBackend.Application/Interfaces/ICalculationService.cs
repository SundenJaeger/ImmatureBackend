namespace ImmatureBackend.Application.Interfaces;

public interface ICalculationService
{
    decimal CalculatePercentage(decimal weight);
    string AssignGrade(decimal percentage);
}