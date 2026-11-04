namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomEvening
{
    public class RoomEveningModel
    {
        public const string BellId = "room-bell";

        public string Room { get; set; } = "12";

        public string? RoomNote { get; set; } = "Quiet evening; Margaret in good spirits.";

        public string? Visitor { get; set; }

        public string? Relationship { get; set; }
    }

    public sealed record RoomLogSaved(string Confirmation);

    public sealed record VisitSigned(string Confirmation);

    public static class RoomEveningEvents
    {
        public static readonly AppEvent ShiftChanged = AppEvent.Declare();
    }

    public class RoomLogValidator : ReactiveValidator<RoomEveningModel>
    {
        public RoomLogValidator()
        {
            RuleFor(x => x.RoomNote).NotEmpty().WithMessage("The room note is required.");
        }
    }

    public class FamilyVisitValidator : ReactiveValidator<RoomEveningModel>
    {
        public FamilyVisitValidator()
        {
            RuleFor(x => x.Visitor).NotEmpty().WithMessage("The visitor's name is required.");
        }
    }
}
