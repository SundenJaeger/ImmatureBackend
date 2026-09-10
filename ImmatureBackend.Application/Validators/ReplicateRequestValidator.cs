using FluentValidation;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class ReplicateRequestValidator : AbstractValidator<ReplicateRequest>
{
    public ReplicateRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Image)
            .NotNull().WithMessage("Image is required.")
            .Must(image => image!.Length > 0).WithMessage("Image cannot be empty.")
            .Must(image => image!.Length <= 10 * 1024 * 1024).WithMessage("Image cannot be larger than 10 MB.");

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