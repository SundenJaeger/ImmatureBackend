using FluentValidation;
using ImmatureBackend.Application.Constants;
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
            .Must(image => image!.Length <= ReplicateRules.MaxImageBytes).WithMessage("Image cannot be larger than 10 MB.");

        RuleFor(request => request.TechnicianName)
            .NotEmpty().WithMessage("Technician Name is required.")
            .MaximumLength(ReplicateRules.TechnicianNameMaxLength)
            .WithMessage($"Technician Name cannot exceed {ReplicateRules.TechnicianNameMaxLength} characters.");

        RuleFor(request => request.SampleId)
            .NotEmpty().WithMessage("Sample ID is required.");

        RuleFor(request => request.Season)
            .NotEmpty().WithMessage("Season is required.")
            .Matches(ReplicateRules.SeasonPattern)
            .WithMessage("Season must be a four-digit year followed by DS or WS, for example 2026WS.");

        RuleFor(request => request.IndexNumber)
            .NotEmpty().WithMessage("Index Number is required.")
            .MaximumLength(ReplicateRules.IndexNumberMaxLength)
            .WithMessage($"Index Number cannot exceed {ReplicateRules.IndexNumberMaxLength} characters.");

        RuleFor(request => request.SelectionName)
            .NotEmpty().WithMessage("Selection Name is required.")
            .MaximumLength(ReplicateRules.SelectionNameMaxLength)
            .WithMessage($"Selection Name cannot exceed {ReplicateRules.SelectionNameMaxLength} characters.");

        RuleFor(request => request.SampleNo)
            .NotNull().WithMessage("Sample No is required.")
            .InclusiveBetween(ReplicateRules.MinSampleNo, ReplicateRules.MaxSampleNo)
            .WithMessage($"Sample No must be {ReplicateRules.MinSampleNo} or {ReplicateRules.MaxSampleNo}.");

        RuleFor(request => request.AiPredictedGrains)
            .NotEmpty().WithMessage("AI Predicted Grains is required.");

        RuleFor(request => request.ConfirmedGrains)
            .NotEmpty().WithMessage("Confirmed Grains is required.");

        RuleFor(request => request.Weight)
            .NotNull().WithMessage("Weight is required.")
            .GreaterThan(0).WithMessage("Weight must be greater than zero.")
            .LessThanOrEqualTo(ReplicateRules.SampleWeightGrams)
            .WithMessage($"Weight cannot be more than {ReplicateRules.SampleWeightGrams:0} g.");

        RuleFor(request => request.CapturedAt)
            .NotNull().WithMessage("Captured At is required.");

        RuleFor(request => request.SubmittedAt)
            .NotNull().WithMessage("Submitted At is required.")
            .Must((request, submittedAt) => request.CapturedAt is null || submittedAt >= request.CapturedAt)
            .WithMessage("Submitted At cannot be earlier than Captured At.");
    }
}