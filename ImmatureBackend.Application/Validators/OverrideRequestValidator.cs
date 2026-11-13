using FluentValidation;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class OverrideRequestValidator : AbstractValidator<OverrideRequest>
{
    public OverrideRequestValidator()
    {
        RuleFor(request => request.Overridden)
            .NotNull().WithMessage("Overridden is required.");
    }
}