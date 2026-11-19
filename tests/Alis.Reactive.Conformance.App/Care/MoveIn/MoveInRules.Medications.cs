using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed class MedicationRules : ReactiveValidator<MedicationInput>
{
    internal MedicationRules(string? ruleSet = null)
    {
        if (ruleSet is null) AddRules();
        else RuleSet(ruleSet, AddRules);
    }

    private void AddRules()
    {
        RuleLevelCascadeMode = CascadeMode.Continue;
        RuleFor(m => m.DrugCode).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Enter a drug code.")
            .Matches("^[A-Z0-9]+$").WithMessage("Use the catalogue drug code.");
        RuleFor(m => m.Dose).GreaterThan(0m).LessThanOrEqualTo(1000m)
            .PrecisionScale(6, 2, true).WithMessage("Use at most two decimal places for the dose.");
        RuleFor(m => m.Schedule).NotEmpty().WithMessage("Choose a medication round.");
        RuleFor(m => m.EndsOn).GreaterThanOrEqualTo(m => m.StartsOn).WithMessage("The prescription cannot end before it starts.");
    }
}

internal sealed class MedicationSectionRules : ReactiveValidator<MedicationSection>
{
    internal MedicationSectionRules(string? ruleSet = null)
    {
        void AddRules()
        {
            RuleForEach(m => m.Rows).SetValidator(new MedicationRules(ruleSet));
            RuleForEach(m => m.RoundDoses).GreaterThanOrEqualTo(m => m.MinimumRoundDose).WithMessage("Each round dose must meet the minimum.");
        }

        if (ruleSet is null) AddRules();
        else RuleSet(ruleSet, AddRules);
    }
}

internal sealed class MedicationEditRules : ReactiveValidator<MedicationEdit>
{
    public MedicationEditRules()
    {
        RuleFor(m => m.Medications).SetValidator(new MedicationSectionRules());
        RuleFor(m => m.NewMedication!).SetValidator(new MedicationRules());
    }
}

public sealed partial class MoveInRules
{
    internal const string Pharmacy = "Pharmacy";

    partial void TheMedications() => RuleSet(Pharmacy, () => RuleFor(m => m.Medications).SetValidator(new MedicationSectionRules(Pharmacy)));
}

internal sealed class PharmacyAllergyRules : ReactiveValidator<PharmacyRequest>
{
    internal PharmacyAllergyRules(IReadOnlyList<MedicationRefusal> failures)
    {
        RuleFor(m => m.Medications).CustomAsync((medications, context, token) =>
        {
            foreach (var failure in failures)
            {
                var at = context.Model.Medications.Rows.FindIndex(row => row.Id == failure.MedicationId);
                context.AddFailure(m => m.Medications.Rows[at].DrugCode, failure.Message);
            }
            return Task.CompletedTask;
        });
    }
}

internal static partial class MoveInValidators
{
    static partial void ForTheMedications(ReactiveFluentValidationBuilder validators)
        => validators.Add<MedicationEditRules>();
}
