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
        public string? Slot { get; set; }

        public string? Family { get; set; }

        public string Message { get; set; } = "";
    }

    /// <summary>What the board's change log is told after the planner answers a push.</summary>
    public sealed class BoardChange
    {
        public string Summary { get; set; } = "";

        public int Booked { get; set; }
    }

    public sealed class TakeRequest
    {
        public string? Family { get; set; }
    }

    /// <summary>The sandbox's slugify plugin: "Garden wing" reads "garden-wing".</summary>
    public sealed class VisitBoardSlugPlugin : Plugin
    {
        public VisitBoardSlugPlugin()
            : base("slugify")
        {
            Slug = Function<string>().Arg<string>();
        }

        public PluginFunction<string> Slug { get; }
    }
}
