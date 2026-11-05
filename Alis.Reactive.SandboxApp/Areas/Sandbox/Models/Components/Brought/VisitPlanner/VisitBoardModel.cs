using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class VisitBoardModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();

        public string? DeskCode { get; set; }
    }

    public sealed class SlotTaken
    {
        public string Slot { get; set; } = "";

        public string Family { get; set; } = "";

        public string Message { get; set; } = "";
    }

    public sealed class DesksWatching
    {
        public int Watching { get; set; }

        public string Summary { get; set; } = "";
    }

    public sealed class BoardChange
    {
        public string Summary { get; set; } = "";

        public int Booked { get; set; }

        public string Desk { get; set; } = "";

        public bool ReleasedHere { get; set; }

        public int Released { get; set; }
    }

    public sealed class VisitBoardQuery
    {
        public string Resident { get; set; } = "";

        public string? Family { get; set; }
    }

    public class TakeRequest
    {
        public string? Family { get; set; }

        public int WeekendVisits { get; set; }
    }

    public sealed class TakeVisitRequest : TakeRequest
    {
        public string? SlotId { get; set; }
    }

    public sealed class VisitBoardSlugPlugin : Plugin
    {
        public VisitBoardSlugPlugin()
            : base("slugify")
        {
        }

        public TypedSource<string> Slug(Arg<string> text) => Method<string>(text, name: "");
    }

    public sealed class VisitLogPlugin : Plugin
    {
        public VisitLogPlugin()
            : base("visitLog")
        {
        }

        public void Record(Arg<string> line) => Method(line);

        public ReadOnly<int> Entries => ReadOnly<int>();
    }

    public sealed class BoardAnnouncerPlugin : Plugin
    {
        public BoardAnnouncerPlugin()
            : base("announce")
        {
        }

        public void Say(Arg<string> line) => Method(line, name: "");
    }

    public sealed class VisitBoardStream : ServerPushStream
    {
        public Event<SlotTakenArgs> SlotTaken => Event<SlotTakenArgs>();

        public Event<DesksWatchingArgs> DesksWatching => Event<DesksWatchingArgs>();
    }

    public sealed class SlotTakenArgs : JsObject
    {
        public ReadOnly<string> Slot => ReadOnly<string>();

        public ReadOnly<string> Family => ReadOnly<string>();

        public ReadOnly<string> Message => ReadOnly<string>();
    }

    public sealed class DesksWatchingArgs : JsObject
    {
        public ReadOnly<int> Watching => ReadOnly<int>();

        public ReadOnly<string> Summary => ReadOnly<string>();
    }

    public static class VisitBoardEvents
    {
        public static readonly AppEvent<BoardChange> BoardChanged = AppEvent.Declare<BoardChange>();
    }
}
