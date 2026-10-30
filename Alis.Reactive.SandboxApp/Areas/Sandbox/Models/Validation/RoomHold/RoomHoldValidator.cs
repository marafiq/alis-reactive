using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomHoldValidator : ReactiveValidator<RoomHoldModel>
    {
        public const string HeldRoom = "Maple 214";

        public RoomHoldValidator()
        {
            // The room travels in a hidden field with no message slot of its own, so the page names it
            // in the validation summary.
            RuleFor(x => x.RoomCode).NotEmpty().WithMessage("Choose a room to hold.");

            RuleFor(x => x.ProspectName).NotEmpty().WithMessage("Enter the prospective resident's name.");

            RuleFor(x => x.FamilyAgreedToHoldTerms)
                .Equal(true).WithMessage("The family must agree that the hold lapses after 48 hours.");

            // Only the server knows which rooms are held: it asks the hold book, which answers
            // asynchronously, so this rule has no browser half.
            RuleFor(x => x.RoomCode)
                .MustAsync(async (room, cancellation) => !await IsHeldForAnotherFamilyAsync(room, cancellation))
                .WithMessage($"{HeldRoom} is already held for another family. Choose another room.");
        }

        // The community's hold book: Maple 214 is held for another family this week.
        private static Task<bool> IsHeldForAnotherFamilyAsync(string? room, CancellationToken cancellation) =>
            Task.FromResult(room == HeldRoom);
    }
}
