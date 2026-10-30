using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The outing's own rules: what the page checks whether or not a companion is on it.</summary>
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

    /// <summary>The companion's rules: what the companion partial brings to the form, and takes away with it.</summary>
    public class OutingCompanionRules : ReactiveValidator<OutingModel>
    {
        public OutingCompanionRules()
        {
            RuleFor(x => x.Companion!).SetValidator(new OutingCompanionValidator());
        }
    }
}
