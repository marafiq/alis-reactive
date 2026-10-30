using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts
{
    /// <summary>A contact card's own rules, checked on the card under whichever prefix it renders.</summary>
    public class MoveInContactValidator : ReactiveValidator<MoveInContact>
    {
        public MoveInContactValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Give a phone number.");
        }
    }
}
