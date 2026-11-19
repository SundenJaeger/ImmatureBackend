using FluentValidation.TestHelper;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Application.Validators;

namespace ImmatureBackend.UnitTest;

public class GradeRequestValidatorTests
{
    private readonly GradeRequestValidator _validator = new();

    [Fact]
    public void Validate_NullWeight_FailsWithRequiredMessageOnly()
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = null });

        result.ShouldHaveValidationErrorFor(request => request.Weight)
            .WithErrorMessage("Weight is required.");
        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.01)]
    [InlineData(-5.0)]
    public void Validate_ZeroOrNegativeWeight_FailsWithGreaterThanZeroMessage(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldHaveValidationErrorFor(request => request.Weight)
            .WithErrorMessage("Weight must be greater than zero.");
    }

    [Theory]
    [InlineData(30.01)]
    [InlineData(31.0)]
    [InlineData(1000.0)]
    public void Validate_WeightAboveThirtyGrams_FailsWithMaximumMessage(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldHaveValidationErrorFor(request => request.Weight)
            .WithErrorMessage("Weight cannot be more than 30 g.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(0.47)]
    [InlineData(15.0)]
    [InlineData(30.0)]
    public void Validate_WeightWithinRange_Passes(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
