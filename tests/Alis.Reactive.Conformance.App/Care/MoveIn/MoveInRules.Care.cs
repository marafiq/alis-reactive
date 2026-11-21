using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules
{
    partial void TheCare()
    {
        RuleFor(moveIn => moveIn.Care.Level).IsInEnum().WithName("Care level");
        When(moveIn => moveIn.Care.Level == CareLevel.Assisted, () =>
        {
            RuleFor(moveIn => moveIn.Care.Assisted).NotNull().WithMessage("Answer the assisted care questions.");
            RuleFor(moveIn => moveIn.Care.Assisted!).SetValidator(new AssistedCareRules());
        });
        When(moveIn => moveIn.Care.Level == CareLevel.Memory, () =>
        {
            RuleFor(moveIn => moveIn.Care.Memory).NotNull().WithMessage("Answer the memory care questions.");
            RuleFor(moveIn => moveIn.Care.Memory!).SetValidator(new MemoryCareRules());
        });
        When(moveIn => moveIn.Care.Level == CareLevel.Skilled, () =>
        {
            RuleFor(moveIn => moveIn.Care.Skilled).NotNull().WithMessage("Answer the skilled care questions.");
            RuleFor(moveIn => moveIn.Care.Skilled!).SetValidator(new SkilledCareRules());
        });
    }
}

public sealed class AssistedCareRules : ReactiveValidator<AssistedCareInput>
{
    public AssistedCareRules()
    {
        RuleFor(assisted => assisted.AssessmentCode).NotEmpty().WithMessage("Enter the assessment code.").Matches("^AS-[0-9]+$").WithMessage("An assisted assessment code is AS- and a number.");
        RuleFor(assisted => assisted.AssessmentDate).NotEmpty().WithMessage("Choose the day of the assessment.").ServerOnly();
        RuleFor(assisted => assisted.AssistanceHours).InclusiveBetween(0m, 24m).WithMessage("Assistance is 0 to 24 hours a day.").PrecisionScale(4, 2, true).WithMessage("Give the hours to two decimal places.");
    }
}

public sealed class MemoryCareRules : ReactiveValidator<MemoryCareInput>
{
    public MemoryCareRules()
    {
        RuleFor(memory => memory.AssessmentCode).NotEmpty().WithMessage("Enter the assessment code.");
        RuleFor(memory => memory.SecureUnitConsent).Equal(true).WithMessage("Secure unit consent is required.");
    }
}

public sealed class SkilledCareRules : ReactiveValidator<SkilledCareInput>
{
    public SkilledCareRules()
        => RuleFor(skilled => skilled.OrderReference).NotEmpty().WithMessage("Enter the order reference.").MinimumLength(5).WithMessage("An order reference holds five letters or more.");
}
