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

    public static TheoryData<decimal> InvalidWeights => new()
    {
        0.0m,
        -0.01m,
        -5.0m
    };

    [Theory]
    [MemberData(nameof(InvalidWeights))]
    public void Validate_ZeroOrNegativeWeight_FailsWithGreaterThanZeroMessage(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldHaveValidationErrorFor(request => request.Weight)
            .WithErrorMessage("Weight must be greater than zero.");
    }

    public static TheoryData<decimal> ExcessiveWeights => new()
    {
        30.01m,
        31.0m,
        1000.0m
    };

    [Theory]
    [MemberData(nameof(ExcessiveWeights))]
    public void Validate_WeightAboveThirtyGrams_FailsWithMaximumMessage(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldHaveValidationErrorFor(request => request.Weight)
            .WithErrorMessage("Weight cannot be more than 30 g.");
    }

    public static TheoryData<decimal> ValidWeights => new()
    {
        0.01m,
        0.47m,
        15.0m,
        30.0m
    };

    [Theory]
    [MemberData(nameof(ValidWeights))]
    public void Validate_WeightWithinRange_Passes(decimal weight)
    {
        var result = _validator.TestValidate(new GradeRequest { Weight = weight });

        result.ShouldNotHaveAnyValidationErrors();
    }
}