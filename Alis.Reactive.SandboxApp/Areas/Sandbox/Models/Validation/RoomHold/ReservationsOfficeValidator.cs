using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The reservations office's rules for a hold, registered by the office the way a module registers its
    /// own. Its rule asks the office's hold book, C# that only the server runs, so the page never checks it.
    /// </summary>
    public class ReservationsOfficeValidator : ReactiveValidator<RoomHoldModel>
    {
        public const string ProspectWithHold = "Ruth Carter";

        public ReservationsOfficeValidator()
        {
            // One hold per prospect: a family releases its hold before placing another.
            RuleFor(x => x.ProspectName)
                .Must(prospect => !HoldsARoom(prospect))
                .WithMessage($"{ProspectWithHold} already holds Maple 218. Release that hold before placing another.");
        }

        // The office's hold book: Ruth Carter holds Maple 218 this week.
        private static bool HoldsARoom(string? prospect) => prospect == ProspectWithHold;
    }
}
