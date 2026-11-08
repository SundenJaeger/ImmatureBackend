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
    [InlineData(0.01, 0.0)]
    [InlineData(0.57, 1.9)]
    [InlineData(0.6, 2.0)]
    [InlineData(1.5, 5.0)]
    [InlineData(3.0, 10.0)]
    [InlineData(4.5, 15.0)]
    [InlineData(4.53, 15.1)]
    [InlineData(30.0, 100.0)]
    public void CalculatePercentage_ReturnsPercentageOfThirtyGramsToOneDecimal(decimal weight, decimal expected)
    {
        var result = _service.CalculatePercentage(weight);

        Assert.Equal(expected, result);
    }
    
    [Theory]
    [InlineData(0.045, 0.2)]
    [InlineData(0.075, 0.3)]
    [InlineData(0.135, 0.5)]
    public void CalculatePercentage_MidpointRoundsAwayFromZero(decimal weight, decimal expected)
    {
        var result = _service.CalculatePercentage(weight);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.0, Grade.Pr)]
    [InlineData(1.9, Grade.Pr)]
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
    [InlineData(15.01, Grade.NotClassified)]
    [InlineData(15.1, Grade.NotClassified)]
    [InlineData(100.0, Grade.NotClassified)]
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

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(1.9, true)]
    [InlineData(1.99, true)]
    [InlineData(2.0, false)]
    [InlineData(5.0, false)]
    [InlineData(15.0, false)]
    [InlineData(15.1, false)]
    public void MeetsRecommendedValue_IsTrueOnlyBelowTwoPercent(decimal percentage, bool expected)
    {
        Assert.Equal(expected, _service.MeetsRecommendedValue(percentage));
    }
}