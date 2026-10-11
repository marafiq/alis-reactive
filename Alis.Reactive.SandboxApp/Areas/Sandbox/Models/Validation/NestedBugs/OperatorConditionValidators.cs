using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs
{
    public class OperatorConditionValidator : ReactiveValidator<OperatorConditionModel>
    {
        public OperatorConditionValidator()
        {
            When(x => x.Age >= 18, () =>
            {
                RuleFor(x => x.JobTitle).NotEmpty().WithMessage("Adults must provide job title.");
            });

            When(x => x.Age < 18, () =>
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Guardian name required for minors.");
            });

            When(x => new[] { "memory-care", "skilled-nursing" }.Contains(x.CareLevel), () =>
            {
                RuleFor(x => x.Notes).NotEmpty().WithMessage("Notes required for high-acuity care.");
            });

            When(x => x.Notes != null && x.Notes.Contains("urgent"), () =>
            {
                RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone required for urgent cases.");
            });

            When(x => !string.IsNullOrEmpty(x.Email), () =>
            {
                RuleFor(x => x.Name).NotEmpty().WithMessage("Name required when email provided.");
            });
        }
    }
}
