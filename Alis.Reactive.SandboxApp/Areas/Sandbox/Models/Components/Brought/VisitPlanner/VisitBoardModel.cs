using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the community's visit board. Each desk opens the board for the family it serves; when a
    // family at another desk takes a time, every other desk's planner shows it at once. The board also
    // reads the week back off the planner: how many weekend visits are booked, how many times are left,
    // and the wing's board code.
    public class VisitBoardModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();

        /// <summary>The desk's code, made from the wing's name: what the desk reads out when a family calls.</summary>
        public string? DeskCode { get; set; }
    }

    /// <summary>Pushed to every other desk when a family takes a time.</summary>
    public sealed class SlotTaken
    {
        public string Slot { get; set; } = "";

        public string Family { get; set; } = "";

        public string Message { get; set; } = "";
    }

    /// <summary>Pushed to every desk on the board when a desk opens it or leaves it.</summary>
    public sealed class DesksWatching
    {
        public int Watching { get; set; }

        public string Summary { get; set; } = "";
    }

    /// <summary>What the board's change log is told after the planner answers a push.</summary>
    public sealed class BoardChange
    {
        public string Summary { get; set; } = "";

        public int Booked { get; set; }

        /// <summary>The code of the desk that saw the change.</summary>
        public string Desk { get; set; } = "";

        /// <summary>Whether the change took a time this family had booked.</summary>
        public bool ReleasedHere { get; set; }

        /// <summary>How many of this family's booked times the change took.</summary>
        public int Released { get; set; }
    }

    /// <summary>The board page's link: <c>?resident=margaret&amp;family=hale</c>.</summary>
    public sealed class VisitBoardQuery
    {
        public string Resident { get; set; } = "";

        public string? Family { get; set; }
    }

    /// <summary>The body of a take, which the TakeVisit endpoint binds with [FromBody].</summary>
    public class TakeRequest
    {
        public string? Family { get; set; }

        /// <summary>How many of the family's booked times fall on a weekend.</summary>
        public int WeekendVisits { get; set; }
    }

    /// <summary>A take as the board sends it to the TakeVisit endpoint: the time in the path, the family in the body.</summary>
    public sealed class TakeVisitRequest : TakeRequest
    {
        public string? SlotId { get; set; }
    }

    /// <summary>The sandbox's slugify plugin: "Garden wing" reads "garden-wing".</summary>
    public sealed class VisitBoardSlugPlugin : Plugin
    {
        public VisitBoardSlugPlugin()
            : base("slugify")
        {
        }

        /// <summary>The plugin is itself the function: it takes text and returns its slug.</summary>
        public TypedSource<string> Slug(Arg<string> text) => Method<string>(text, name: "");
    }

    /// <summary>The board's activity list (Scripts/sandbox-plugins.ts): each line recorded shows in it.</summary>
    public sealed class VisitLogPlugin : Plugin
    {
        public VisitLogPlugin()
            : base("visitLog")
        {
        }

        /// <summary>Adds a line to the activity list.</summary>
        public void Record(Arg<string> line) => Method(line);

        /// <summary>How many lines the activity list holds.</summary>
        public ReadOnly<int> Entries => ReadOnly<int>();
    }

    /// <summary>The board's spoken status line, for screen readers and the desk alike.</summary>
    public sealed class BoardAnnouncerPlugin : Plugin
    {
        public BoardAnnouncerPlugin()
            : base("announce")
        {
        }

        /// <summary>The plugin is itself the function: it reads a line out in the board's status region.</summary>
        public void Say(Arg<string> line) => Method(line, name: "");
    }

    /// <summary>
    /// The visit board's stream: a time another desk's family took, and how many desks are watching the
    /// week. Each event has its own name; a trigger hears only the one it names.
    /// </summary>
    public sealed class VisitBoardStream : ServerPushStream
    {
        public Event<SlotTakenArgs> SlotTaken => Event<SlotTakenArgs>();

        public Event<DesksWatchingArgs> DesksWatching => Event<DesksWatchingArgs>();
    }

    /// <summary>What a desk reads of a <see cref="SlotTaken"/> push.</summary>
    public sealed class SlotTakenArgs : JsObject
    {
        public ReadOnly<string> Slot => ReadOnly<string>();

        public ReadOnly<string> Family => ReadOnly<string>();

        public ReadOnly<string> Message => ReadOnly<string>();
    }

    /// <summary>What a desk reads of a <see cref="DesksWatching"/> push.</summary>
    public sealed class DesksWatchingArgs : JsObject
    {
        public ReadOnly<int> Watching => ReadOnly<int>();

        public ReadOnly<string> Summary => ReadOnly<string>();
    }

    /// <summary>The document events the VisitBoard page raises and hears.</summary>
    public static class VisitBoardEvents
    {
        public static readonly AppEvent<BoardChange> BoardChanged = AppEvent.Declare<BoardChange>();
    }
}
