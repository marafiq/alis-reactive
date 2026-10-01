using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomHoldValidator : ReactiveValidator<RoomHoldModel>
    {
        public const string HeldRoom = "Maple 214";

        public RoomHoldValidator()
        {
            ClientRule(x => x.ProspectName).Required("Enter the prospective resident's name.");

            // Only the server knows which rooms are held, so this rule has no browser half.
            RuleFor(x => x.RoomCode)
                .Must(room => room != HeldRoom)
                .WithMessage($"{HeldRoom} is already held for another family. Choose another room.");
        }
    }
}
