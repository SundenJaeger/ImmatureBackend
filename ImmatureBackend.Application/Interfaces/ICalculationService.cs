using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Interfaces;

public interface ICalculationService
{
    decimal CalculatePercentage(decimal weight);
    Grade AssignGrade(decimal percentage);
}