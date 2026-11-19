using FluentValidation;
using ImmatureBackend.Application.Constants;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class GradeRequestValidator : AbstractValidator<GradeRequest>
{
    public GradeRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Weight)
            .NotNull().WithMessage("Weight is required.")
            .GreaterThan(0).WithMessage("Weight must be greater than zero.")
            .LessThanOrEqualTo(ReplicateRules.SampleWeightGrams)
            .WithMessage($"Weight cannot be more than {ReplicateRules.SampleWeightGrams:0} g.");
    }
}
