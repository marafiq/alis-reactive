namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ReservationsOfficeValidator : ReactiveValidator<RoomHoldModel>
    {
        public const string ProspectWithHold = "Ruth Carter";

        public ReservationsOfficeValidator()
        {
            RuleFor(x => x.ProspectName)
                .Must(prospect => !HoldsARoom(prospect))
                .WithMessage($"{ProspectWithHold} already holds Maple 218. Release that hold before placing another.");
        }

        private static bool HoldsARoom(string? prospect) => prospect == ProspectWithHold;
    }
}
