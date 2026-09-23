using ImmatureBackend.Application.Services;
using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Test.Application.Services;

public class CalculationServiceTests
{
    private readonly CalculationService _service = new();

    [Fact]
    public void CalculatePercentage_ZeroWeight_ShouldNotDivideByOrReturnAnything_ReturnsZero()
    {
        var result = _service.CalculatePercentage(0m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculatePercentage_NegativeWeight_ShouldNotReturnNegativePercentage_ReturnsZero()
    {
        var result = _service.CalculatePercentage(-10m);

        Assert.Equal(0m, result);
    }

    [Theory]
    [InlineData(0.0, Grade.Pr)]
    [InlineData(1.99, Grade.Pr)]
    [InlineData(2.0, Grade.G1)]
    [InlineData(3.5, Grade.G1)]
    [InlineData(5.0, Grade.G1)]
    [InlineData(5.01, Grade.G2)]
    [InlineData(7.5, Grade.G2)]
    [InlineData(10.0, Grade.G2)]
    [InlineData(10.01, Grade.G3)]
    [InlineData(12.5, Grade.G3)]
    [InlineData(15.0, Grade.G3)]
    [InlineData(15.01, Grade.BelowStandard)]
    [InlineData(100.0, Grade.BelowStandard)]
    public void AssignGrade_ReturnsCorrectGradeAtEachBoundary(decimal percentage, Grade expectedGrade)
    {
        var result = _service.AssignGrade(percentage);

        Assert.Equal(expectedGrade, result);
    }

    [Fact]
    public void AssignGrade_NegativePercentage_ShouldNotThrow_FallsIntoLowestGrade()
    {
        var result = _service.AssignGrade(-50m);

        Assert.Equal(Grade.Pr, result);
    }

    [Fact]
    public void AssignGrade_ExactlyOnUpperBoundary_ShouldNotRoundUpToNextGrade()
    {
        Assert.Equal(Grade.G1, _service.AssignGrade(5.0m));
        Assert.NotEqual(Grade.G2, _service.AssignGrade(5.0m));
    }
}