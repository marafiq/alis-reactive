namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class RoomTransferValidator : ReactiveValidator<RoomTransferModel>
    {
        public static readonly IReadOnlyList<string> OccupiedRooms = new[] { "MA-220", "MC-220" };

        public RoomTransferValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("Enter the resident's name.");
            RuleFor(x => x.FromRoom).NotEmpty().WithMessage("Enter the resident's current room.");

            RuleFor(x => x.ToRoom)
                .NotEmpty().WithMessage("Enter the room the resident moves to.")
                .NotEqual(x => x.FromRoom).WithMessage("The resident is already in that room.")
                .Must(room => !OccupiedRooms.Contains(room ?? "")).WithMessage(transfer => $"{transfer.ToRoom} is occupied.")
                .Must((transfer, room) => OnTheSameWing(transfer.FromRoom, room)).WithMessage("Transfers stay on the resident's wing.");

            RuleFor(x => x.TransferDate)
                .NotNull().WithMessage("Choose the transfer day.")
                .MustAsync((day, cancellation) => HousekeepingTurnsRoomsOnAsync(day, cancellation))
                .WithMessage("Housekeeping does not turn rooms on a Sunday.");

            RuleFor(x => x.Reason).MaximumLength(100).WithMessage("Keep the reason under 100 characters.").ServerOnly();

            RuleFor(x => x.Notes).MaximumLength(20).WithMessage(transfer => $"{transfer.ResidentName}'s notes run over 20 characters.");

            WhenOnServer(x => FallsOnAWeekend(x.TransferDate), () =>
                RuleFor(x => x.WeekendApprover).NotEmpty().WithMessage("A weekend transfer needs the administrator on call to approve it."));
        }

        private static bool OnTheSameWing(string? from, string? to) =>
            from == null || to == null || from.Length < 2 || to.Length < 2 || from.Substring(0, 2) == to.Substring(0, 2);

        private static Task<bool> HousekeepingTurnsRoomsOnAsync(DateTime? day, CancellationToken cancellation) =>
            Task.FromResult(day?.DayOfWeek != DayOfWeek.Sunday);

        private static bool FallsOnAWeekend(DateTime? day) =>
            day is { DayOfWeek: DayOfWeek.Saturday or DayOfWeek.Sunday };
    }
}
