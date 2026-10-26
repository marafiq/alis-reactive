using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The rules every controlled substance line adds: a second nurse verified the count.</summary>
    public class ControlledSubstanceRules : ReactiveValidator<MedicationLine>
    {
        public ControlledSubstanceRules()
        {
            RuleFor(x => x.CountVerifiedBy).NotEmpty().WithMessage("A second nurse verifies a controlled substance count: name them.");
        }
    }

    public class MedicationLineValidator : ReactiveValidator<MedicationLine>
    {
        public MedicationLineValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name the medication.");
            RuleFor(x => x.DoseMg).GreaterThan(0).WithMessage("The dose is above 0 mg.");
            RuleFor(x => x.Hours).Matches(@"^\d{1,2}(,\d{1,2})*$").WithMessage("Enter the hours given, like 8,20.");

            // Rules shared by every controlled substance, included only on a controlled line.
            When(x => x.Controlled, () => Include(new ControlledSubstanceRules()));

            RuleFor(x => x.Prescriber).ChildRules(prescriber =>
                prescriber.RuleFor(x => x.Npi).Matches(@"^\d{10}$").WithMessage("A prescriber's NPI is ten digits."));
        }
    }

    public class MedicationReconciliationValidator : ReactiveValidator<MedicationReconciliationModel>
    {
        public MedicationReconciliationValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleForEach(x => x.Medications).SetValidator(new MedicationLineValidator());

            // A check on each allergy line itself, when the resident has allergies.
            When(x => x.HasAllergies, () =>
                RuleForEach(x => x.Allergies).NotEmpty().WithMessage("Name the allergy or clear the line."));

            RuleFor(x => x.AdministrationChecks).NotEmpty().WithMessage("Tick at least one administration check.");
        }
    }
}
