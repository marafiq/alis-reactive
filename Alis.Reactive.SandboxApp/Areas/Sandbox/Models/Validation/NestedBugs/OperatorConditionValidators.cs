using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    public class OperatorConditionValidator : ReactiveValidator<OperatorConditionModel>
    {
        public OperatorConditionValidator()
        {
            WhenFieldGte(x => x.Age, 18, () =>
            {
                ClientRule(x => x.JobTitle)
                    .Required("Adults must provide job title.");
            });

            WhenFieldLt(x => x.Age, 18, () =>
            {
                ClientRule(x => x.Name)
                    .Required("Guardian name required for minors.");
            });

            WhenFieldIn(x => x.CareLevel, new[] { "memory-care", "skilled-nursing" }, () =>
            {
                ClientRule(x => x.Notes)
                    .Required("Notes required for high-acuity care.");
            });

            WhenFieldContains(x => x.Notes, "urgent", () =>
            {
                ClientRule(x => x.Phone)
                    .Required("Phone required for urgent cases.");
            });

            WhenFieldNotEmpty(x => x.Email, () =>
            {
                ClientRule(x => x.Name)
                    .Required("Name required when email provided.");
            });
        }
    }
}
