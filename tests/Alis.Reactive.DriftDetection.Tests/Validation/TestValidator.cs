using Alis.Reactive.DriftDetection.Tests.Infrastructure;
using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.DriftDetection.Tests.Validation;

/// <summary>
/// Test validator exercising extractable rule types for drift detection.
/// Uses ReactiveValidator for WhenField conditional extraction.
/// </summary>
public class TestValidator : ReactiveValidator<ResidentModel>
{
    public TestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Valid email required");
        RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Care level is required");
        RuleFor(x => x.PhysicianName)
            .Equal(x => x.Name!)
            .WithMessage("Physician must match resident name");

        // Conditional rule: VeteranId required when IsVeteran is truthy
        WhenField(x => x.IsVeteran, () =>
        {
            RuleFor(x => x.VeteranId).NotEmpty().WithMessage("Veteran ID is required");
        });

        // Conditional rule with value: PhysicianName required when CareLevel == "Memory Care"
        WhenField<string>(x => x.CareLevel!, "Memory Care", () =>
        {
            RuleFor(x => x.PhysicianName).NotEmpty().WithMessage("Physician required for Memory Care");
        });

        // Cross-property comparison: MonthlyRate must be > 0
        RuleFor(x => x.MonthlyRate)
            .GreaterThan(0m).WithMessage("Rate must be positive");
    }
}
