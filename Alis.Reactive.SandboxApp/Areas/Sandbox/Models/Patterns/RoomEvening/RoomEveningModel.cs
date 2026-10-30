using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Patterns.RoomEvening
{
    // Journey: the evening aide keeps room 12's evening log. A family visit opens as a card loaded into
    // the log: its own inputs, its own rule and its own buttons work while it is there, it hears the
    // room's bell the page rendered, and it can open an escort card of its own. Ending the visit
    // replaces the card: everything the card brought goes with it, and the room's own bell, buttons and
    // rule keep working.
    public class RoomEveningModel
    {
        /// <summary>The id of the room's bell; the visit card listens to it by this id.</summary>
        public const string BellId = "room-bell";

        /// <summary>The room.</summary>
        public string Room { get; set; } = "12";

        /// <summary>The aide's note for the evening.</summary>
        public string? RoomNote { get; set; } = "Quiet evening; Margaret in good spirits.";

        /// <summary>The visitor signed in, while a visit is open.</summary>
        public string? Visitor { get; set; }

        /// <summary>The visitor's relationship to the resident.</summary>
        public string? Relationship { get; set; }
    }

    /// <summary>The confirmation the server returns once the evening log is saved.</summary>
    public sealed record RoomLogSaved(string Confirmation);

    /// <summary>The confirmation the server returns once a visitor is signed in.</summary>
    public sealed record VisitSigned(string Confirmation);

    /// <summary>The document events the room's evening raises and hears.</summary>
    public static class RoomEveningEvents
    {
        public static readonly AppEvent ShiftChanged = AppEvent.Declare();
    }

    /// <summary>The page's rule: the evening log needs a note.</summary>
    public class RoomLogValidator : ReactiveValidator<RoomEveningModel>
    {
        public RoomLogValidator()
        {
            RuleFor(x => x.RoomNote).NotEmpty().WithMessage("The room note is required.");
        }
    }

    /// <summary>The visit card's rule: a visitor is signed in by name.</summary>
    public class FamilyVisitValidator : ReactiveValidator<RoomEveningModel>
    {
        public FamilyVisitValidator()
        {
            RuleFor(x => x.Visitor).NotEmpty().WithMessage("The visitor's name is required.");
        }
    }
}
