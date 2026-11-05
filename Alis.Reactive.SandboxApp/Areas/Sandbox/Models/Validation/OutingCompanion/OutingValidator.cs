
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class OutingValidator : ReactiveValidator<OutingModel>
    {
        public OutingValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(x => x.Destination).NotEmpty().WithMessage("Say where the outing goes.");
        }
    }

    public class OutingCompanionValidator : ReactiveValidator<OutingCompanion>
    {
        public OutingCompanionValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Enter the companion's name.");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Enter the companion's phone.");
        }
    }

    public class OutingCompanionRules : ReactiveValidator<OutingModel>
    {
        public OutingCompanionRules()
        {
            RuleFor(x => x.Companion!).SetValidator(new OutingCompanionValidator());
        }
    }
}
