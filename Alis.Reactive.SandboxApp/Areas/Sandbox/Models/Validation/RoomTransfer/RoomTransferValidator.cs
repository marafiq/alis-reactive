using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The transfer's rules. The page runs the ones it can decide; the rest stay on the server by what
    /// they are: C# that asks the room book, a lookup that waits, a message written from the model, a
    /// rule kept on the server on purpose, and a condition only C# can evaluate.
    /// </summary>
    public class RoomTransferValidator : ReactiveValidator<RoomTransferModel>
    {
        /// <summary>The rooms the room book shows occupied this week.</summary>
        public static readonly IReadOnlyList<string> OccupiedRooms = new[] { "MA-220", "MC-220" };

        public RoomTransferValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(x => x.FromRoom).NotEmpty().WithMessage("Enter the resident's current room.");

            RuleFor(x => x.ToRoom)
                .NotEmpty().WithMessage("Enter the room the resident moves to.")
                .NotEqual(x => x.FromRoom).WithMessage("The resident is already in that room.")
                // The room book is the server's: C# asks it.
                .Must(room => !OccupiedRooms.Contains(room ?? "")).WithMessage(transfer => $"{transfer.ToRoom} is occupied.")
                .Must((transfer, room) => OnTheSameWing(transfer.FromRoom, room)).WithMessage("Transfers stay on the resident's wing.");

            RuleFor(x => x.TransferDate)
                .NotNull().WithMessage("Choose the transfer day.")
                // Housekeeping's calendar is asked, and the answer waits.
                .MustAsync((day, cancellation) => HousekeepingTurnsRoomsOnAsync(day, cancellation))
                .WithMessage("Housekeeping does not turn rooms on a Sunday.");

            // A rule kept on the server on purpose: the page does not count the reason's characters.
            RuleFor(x => x.Reason).MaximumLength(100).WithMessage("Keep the reason under 100 characters.").ServerOnly();

            // A message written from the model is the server's to say, so the check runs there.
            RuleFor(x => x.Notes).MaximumLength(20).WithMessage(transfer => $"{transfer.ResidentName}'s notes run over 20 characters.");

            // A condition only C# can evaluate keeps its rule on the server.
            WhenOnServer(x => FallsOnAWeekend(x.TransferDate), () =>
                RuleFor(x => x.WeekendApprover).NotEmpty().WithMessage("A weekend transfer needs the administrator on call to approve it."));
        }

        // MA-214 and MA-216 are both on the MA wing; a missing room is another rule's to refuse.
        private static bool OnTheSameWing(string? from, string? to) =>
            from == null || to == null || from.Length < 2 || to.Length < 2 || from.Substring(0, 2) == to.Substring(0, 2);

        private static Task<bool> HousekeepingTurnsRoomsOnAsync(DateTime? day, CancellationToken cancellation) =>
            Task.FromResult(day?.DayOfWeek != DayOfWeek.Sunday);

        private static bool FallsOnAWeekend(DateTime? day) =>
            day is { DayOfWeek: DayOfWeek.Saturday or DayOfWeek.Sunday };
    }
}
