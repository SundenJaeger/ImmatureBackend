using FluentValidation;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class UpdateStatusRequestValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Status)
            .NotEmpty().WithMessage("Review Status is required.");
    }
}