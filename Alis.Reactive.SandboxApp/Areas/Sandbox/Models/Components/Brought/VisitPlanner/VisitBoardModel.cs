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
    }

    /// <summary>Pushed to every other desk when a family takes a time.</summary>
    public sealed class SlotTaken
    {
        public string Slot { get; set; } = "";

        public string Family { get; set; } = "";

        public string Message { get; set; } = "";
    }

    /// <summary>What the board's change log is told after the planner answers a push.</summary>
    public sealed class BoardChange
    {
        public string Summary { get; set; } = "";

        public int Booked { get; set; }
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
        public TypedSource<string> Slug(Arg<string> text) => Invoke<string>(text, name: "");
    }

    /// <summary>The document events the VisitBoard page raises and hears.</summary>
    /// <summary>The visit board's stream: a time another desk's family took.</summary>
    public sealed class VisitBoardStream : ServerPushStream
    {
        public ServerPushMessage<SlotTaken> SlotTaken => Message<SlotTaken>("slot-taken");
    }

    public static class VisitBoardEvents
    {
        public static readonly AppEvent<BoardChange> BoardChanged = AppEvent.Named<BoardChange>("board-changed");
    }
}
