using ImmatureBackend.Application.Services;

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
    [InlineData(0.0, "Pr")]
    [InlineData(1.99, "Pr")]
    [InlineData(2.0, "G1")]
    [InlineData(3.5, "G1")]
    [InlineData(5.0, "G1")]
    [InlineData(5.01, "G2")]
    [InlineData(7.5, "G2")]
    [InlineData(10.0, "G2")]
    [InlineData(10.01, "G3")]
    [InlineData(12.5, "G3")]
    [InlineData(15.0, "G3")]
    [InlineData(15.01, "Below Standard")]
    [InlineData(100.0, "Below Standard")]
    public void AssignGrade_ReturnsCorrectGradeAtEachBoundary(decimal percentage, string expectedGrade)
    {
        var result = _service.AssignGrade(percentage);

        Assert.Equal(expectedGrade, result);
    }

    [Fact]
    public void AssignGrade_NegativePercentage_ShouldNotThrow_FallsIntoLowestGrade()
    {
        var result = _service.AssignGrade(-50m);

        Assert.Equal("Pr", result);
    }

    [Fact]
    public void AssignGrade_ExactlyOnUpperBoundary_ShouldNotRoundUpToNextGrade()
    {
        Assert.Equal("G1", _service.AssignGrade(5.0m));
        Assert.NotEqual("G2", _service.AssignGrade(5.0m));
    }
}