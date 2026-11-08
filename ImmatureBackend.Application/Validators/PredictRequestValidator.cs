using FluentValidation;
using ImmatureBackend.Application.Constants;
using ImmatureBackend.Application.Requests;

namespace ImmatureBackend.Application.Validators;

public class PredictRequestValidator : AbstractValidator<PredictRequest>
{
    public PredictRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(request => request.Image)
            .NotNull().WithMessage("Image is required.")
            .Must(image => image!.Length > 0).WithMessage("Image cannot be empty.")
            .Must(image => image!.Length <= ReplicateRules.MaxImageBytes).WithMessage("Image cannot be larger than 10 MB.");

        RuleFor(request => request.TechnicianName)
            .NotEmpty().WithMessage("Technician Name is required.")
            .MaximumLength(ReplicateRules.TechnicianNameMaxLength)
            .WithMessage($"Technician Name cannot exceed {ReplicateRules.TechnicianNameMaxLength} characters.");

        RuleFor(request => request.SampleId)
            .NotEmpty().WithMessage("Sample ID is required.");
    }
}