namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomHoldValidator : ReactiveValidator<RoomHoldModel>
    {
        public const string HeldRoom = "Maple 214";

        public RoomHoldValidator()
        {
            RuleFor(x => x.RoomCode).NotEmpty().WithMessage("Choose a room to hold.");

            RuleFor(x => x.ProspectName).NotEmpty().WithMessage("Enter the prospective resident's name.");

            RuleFor(x => x.FamilyAgreedToHoldTerms)
                .Equal(true).WithMessage("The family must agree that the hold lapses after 48 hours.");

            RuleFor(x => x.RoomCode)
                .MustAsync(async (room, cancellation) => !await IsHeldForAnotherFamilyAsync(room, cancellation))
                .WithMessage($"{HeldRoom} is already held for another family. Choose another room.");
        }

        private static Task<bool> IsHeldForAnotherFamilyAsync(string? room, CancellationToken cancellation) =>
            Task.FromResult(room == HeldRoom);
    }
}
