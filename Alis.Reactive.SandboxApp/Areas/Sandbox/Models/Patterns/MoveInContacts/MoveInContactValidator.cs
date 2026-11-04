
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.MoveInContacts
{
    public class MoveInContactValidator : ReactiveValidator<MoveInContact>
    {
        public MoveInContactValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Give a phone number.");
        }
    }
}
