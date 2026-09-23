using ImmatureBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ImmatureBackend.Infrastructure.Persistence.ValueConverters;

public sealed class GradeConverter() : ValueConverter<Grade, string>(grade => ConvertToString(grade),
    value => ConvertToGrade(value))
{
    private static string ConvertToString(Grade grade)
    {
        return grade switch
        {
            Grade.Pr => "Pr",
            Grade.G1 => "G1",
            Grade.G2 => "G2",
            Grade.G3 => "G3",
            Grade.BelowStandard => "Below Standard",
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
        };
    }

    private static Grade ConvertToGrade(string value)
    {
        return value switch
        {
            "Pr" => Grade.Pr,
            "G1" => Grade.G1,
            "G2" => Grade.G2,
            "G3" => Grade.G3,
            "Below Standard" => Grade.BelowStandard,
            _ => throw new ArgumentException(
                $"Unknown Grade: {value}",
                nameof(value))
        };
    }
}