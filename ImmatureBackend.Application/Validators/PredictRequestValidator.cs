using FluentValidation;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class PredictRequestValidator : AbstractValidator<PredictRequest>
{
    public PredictRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Image)
            .NotNull().WithMessage("Image is required.");

        RuleFor(request => request.TechnicianName)
            .NotEmpty().WithMessage("Technician Name is required.");

        RuleFor(request => request.SampleId)
            .NotEmpty().WithMessage("Sample ID is required.");
    }
}