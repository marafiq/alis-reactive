using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    public class OperatorConditionValidator : ReactiveValidator<OperatorConditionModel>
    {
        public OperatorConditionValidator()
        {
            // Gte: age >= 18 → JobTitle required
            WhenFieldGte(x => x.Age, 18, () =>
            {
                RuleFor(x => x.JobTitle).NotEmpty()
                    .WithMessage("Adults must provide job title.");
            });

            // Lt: age < 18 → Name required (guardian)
            WhenFieldLt(x => x.Age, 18, () =>
            {
                RuleFor(x => x.Name).NotEmpty()
                    .WithMessage("Guardian name required for minors.");
            });

            // In: care level in set → Notes required
            WhenFieldIn(x => x.CareLevel, new[] { "memory-care", "skilled-nursing" }, () =>
            {
                RuleFor(x => x.Notes).NotEmpty()
                    .WithMessage("Notes required for high-acuity care.");
            });

            // Contains: notes contain "urgent" → Phone required
            WhenFieldContains(x => x.Notes, "urgent", () =>
            {
                RuleFor(x => x.Phone).NotEmpty()
                    .WithMessage("Phone required for urgent cases.");
            });

            // NotEmpty: email not empty → Name required
            WhenFieldNotEmpty(x => x.Email, () =>
            {
                RuleFor(x => x.Name).NotEmpty()
                    .WithMessage("Name required when email provided.");
            });
        }
    }
}
