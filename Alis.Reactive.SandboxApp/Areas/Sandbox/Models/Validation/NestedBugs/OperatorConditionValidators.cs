using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    public class OperatorConditionValidator : ReactiveValidator<OperatorConditionModel>
    {
        public OperatorConditionValidator()
        {
            // Gte: age >= 18 → JobTitle required
            WhenFieldGte(x => x.Age, 18, () =>
            {
                ClientRule(x => x.JobTitle)
                    .Required("Adults must provide job title.");
            });

            // Lt: age < 18 → Name required (guardian)
            WhenFieldLt(x => x.Age, 18, () =>
            {
                ClientRule(x => x.Name)
                    .Required("Guardian name required for minors.");
            });

            // In: care level in set → Notes required
            WhenFieldIn(x => x.CareLevel, new[] { "memory-care", "skilled-nursing" }, () =>
            {
                ClientRule(x => x.Notes)
                    .Required("Notes required for high-acuity care.");
            });

            // Contains: notes contain "urgent" → Phone required
            WhenFieldContains(x => x.Notes, "urgent", () =>
            {
                ClientRule(x => x.Phone)
                    .Required("Phone required for urgent cases.");
            });

            // NotEmpty: email not empty → Name required
            WhenFieldNotEmpty(x => x.Email, () =>
            {
                ClientRule(x => x.Name)
                    .Required("Name required when email provided.");
            });
        }
    }
}
