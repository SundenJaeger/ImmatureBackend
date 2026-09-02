using FluentValidation;
using ImmatureBackend.Application.Requests;
using ImmatureBackend.Domain.Enums;

namespace ImmatureBackend.Application.Validators;

public class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Status)
            .NotEmpty()
            .WithMessage("Review Status is required.")
            .Must(s => Enum.TryParse<ReviewStatus>(
                s,
                ignoreCase: true,
                out _
            ))
            .WithMessage("Review Status must be a valid review status (eg. review, accepted, rejected, retraining)");
    }
}