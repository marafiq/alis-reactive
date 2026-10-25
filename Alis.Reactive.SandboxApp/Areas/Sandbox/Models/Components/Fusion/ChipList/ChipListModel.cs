using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ChipList
{
    /// <summary>
    /// The activities coordinator plans the next outing: where it goes, the residents' interests it is
    /// tagged with, who rides the bus, and the days it repeats.
    /// </summary>
    public sealed class ChipListModel
    {
        /// <summary>Where the outing goes, by destination id.</summary>
        public string? Destination { get; set; }

        /// <summary>The places the bus can take residents.</summary>
        public IReadOnlyList<OutingDestination> Destinations { get; init; } = [];

        /// <summary>The interests an outing can be tagged with, in the order the tags show.</summary>
        public IReadOnlyList<OutingInterest> Interests { get; init; } = [];

        /// <summary>The residents who asked the front desk to go.</summary>
        public IReadOnlyList<string> AskedToGo { get; init; } = [];

        /// <summary>The residents signed up, in the order they signed up; the first four have seats.</summary>
        public IReadOnlyList<string> Riders { get; init; } = [];

        /// <summary>The days of the week, Sunday first, as <see cref="DayOfWeek"/> numbers them.</summary>
        public IReadOnlyList<string> WeekDays { get; init; } = [];
    }

    /// <summary>A place the bus goes.</summary>
    public sealed class OutingDestination
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>An interest a tag stands for: the value the tag keeps and the name it shows.</summary>
    public sealed class OutingInterest
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>Asks what a destination suits.</summary>
    public sealed class DestinationQuery
    {
        public string? Destination { get; set; }
    }

    /// <summary>The interests a destination suits and the day of the week it runs, as <see cref="DayOfWeek"/> numbers it.</summary>
    public sealed class DestinationOuting
    {
        public string[] Tags { get; init; } = [];

        public int Day { get; init; }
    }

    /// <summary>The trip's tags, by name and by interest, to find whom to invite.</summary>
    public sealed class InviteeQuery
    {
        public string[]? TagNames { get; set; }

        public string[]? Interests { get; set; }
    }

    /// <summary>Whom the front desk invites for the trip's tags.</summary>
    public sealed class InviteeList
    {
        public string Message { get; init; } = "";
    }

    /// <summary>The riders who use the bus's wheelchair lift, by name.</summary>
    public sealed class LiftRiderList
    {
        public string[] Names { get; init; } = [];
    }

    /// <summary>The outing as the coordinator posts it to the activities calendar.</summary>
    public sealed class OutingPost
    {
        public string? Destination { get; set; }

        public string[]? Tags { get; set; }

        public int[]? RepeatDays { get; set; }
    }

    /// <summary>What the activities calendar says about the post.</summary>
    public sealed class OutingPosted
    {
        public string Message { get; init; } = "";
    }

    /// <summary>The trip's tags as the tag list shows them.</summary>
    public sealed class OutingTags
    {
        public FusionSelectedChips Selection { get; set; } = new();
    }

    /// <summary>A seated rider left the bus; the resident now in the last seat, if any, came off the wait list.</summary>
    public sealed class OpenSeat
    {
        public string Rider { get; set; } = "";

        public FusionChipData NextInLine { get; set; } = new();
    }

    /// <summary>The document events the outing planner raises and hears.</summary>
    public static class OutingEvents
    {
        public static readonly AppEvent<OutingTags> Tagged = AppEvent.Declare<OutingTags>();

        public static readonly AppEvent<OpenSeat> SeatOpened = AppEvent.Declare<OpenSeat>();
    }
}
