using FluentValidation;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class ReplicateRequestValidator : AbstractValidator<ReplicateRequest>
{
    public ReplicateRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Image)
            .NotNull().WithMessage("Image is required.");

        RuleFor(request => request.TechnicianName)
            .NotEmpty().WithMessage("Technician Name is required.");

        RuleFor(request => request.SampleId)
            .NotEmpty().WithMessage("Sample ID is required.");

        RuleFor(request => request.AiPredictedGrains)
            .NotEmpty().WithMessage("AI Predicted Grains is required.");

        RuleFor(request => request.ConfirmedGrains)
            .NotEmpty().WithMessage("Confirmed Grains is required.");

        RuleFor(request => request.Weight)
            .NotNull().WithMessage("Weight is required.")
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");
    }
}