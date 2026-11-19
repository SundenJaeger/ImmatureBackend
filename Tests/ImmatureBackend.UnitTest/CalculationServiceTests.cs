using ImmatureBackend.Application.Services;
using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.UnitTest;

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

    public static TheoryData<decimal, decimal> PercentageTestData => new()
    {
        { 0.01m, 0.0m },
        { 0.57m, 1.9m },
        { 0.6m, 2.0m },
        { 1.5m, 5.0m },
        { 3.0m, 10.0m },
        { 4.5m, 15.0m },
        { 4.53m, 15.1m },
        { 30.0m, 100.0m }
    };

    [Theory]
    [MemberData(nameof(PercentageTestData))]
    public void CalculatePercentage_ReturnsPercentageOfThirtyGramsToOneDecimal(decimal weight, decimal expected)
    {
        var result = _service.CalculatePercentage(weight);

        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal, decimal> MidpointTestData => new()
    {
        { 0.045m, 0.2m },
        { 0.075m, 0.3m },
        { 0.135m, 0.5m }
    };

    [Theory]
    [MemberData(nameof(MidpointTestData))]
    public void CalculatePercentage_MidpointRoundsAwayFromZero(decimal weight, decimal expected)
    {
        var result = _service.CalculatePercentage(weight);

        Assert.Equal(expected, result);
    }

    public static TheoryData<decimal, Grade> GradeTestData => new()
    {
        { 0.0m, Grade.Pr },
        { 1.9m, Grade.Pr },
        { 1.99m, Grade.Pr },
        { 2.0m, Grade.G1 },
        { 3.5m, Grade.G1 },
        { 5.0m, Grade.G1 },
        { 5.01m, Grade.G2 },
        { 7.5m, Grade.G2 },
        { 10.0m, Grade.G2 },
        { 10.01m, Grade.G3 },
        { 12.5m, Grade.G3 },
        { 15.0m, Grade.G3 },
        { 15.01m, Grade.NotClassified },
        { 15.1m, Grade.NotClassified },
        { 100.0m, Grade.NotClassified }
    };

    [Theory]
    [MemberData(nameof(GradeTestData))]
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

    public static TheoryData<decimal, bool> IsTrueOnlyBelowTwoPercentTestData => new()
    {
        { 0.0m, true },
        { 1.9m, true },
        { 1.99m, true },
        { 2.0m, false },
        { 5.0m, false },
        { 15.0m, false },
        { 15.1m, false }
    };

    [Theory]
    [MemberData(nameof(IsTrueOnlyBelowTwoPercentTestData))]
    public void MeetsRecommendedValue_IsTrueOnlyBelowTwoPercent(decimal percentage, bool expected)
    {
        Assert.Equal(expected, _service.MeetsRecommendedValue(percentage));
    }
}