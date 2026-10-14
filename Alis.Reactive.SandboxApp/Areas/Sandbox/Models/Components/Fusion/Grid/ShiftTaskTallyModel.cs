using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The shift task tally page: the grid holds the tally, so the page has no fields of its own.</summary>
    public class ShiftTaskTallyModel
    {
        /// <summary>The wing's residents, with the counts the evening shift starts from.</summary>
        public List<ShiftTallyResident> Residents { get; set; } = new();
    }

    /// <summary>A resident's counts for the shift change, one row of the tally.</summary>
    public class ShiftTallyResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public int OpenTasks { get; set; }

        /// <summary>As-needed doses given this shift.</summary>
        public int PrnDoses { get; set; }
    }

    /// <summary>The tally's pending changes, reviewed or sent to the night shift.</summary>
    public class ShiftTallyChanges
    {
        public FusionGridBatchChanges<ShiftTallyResident>? BatchChanges { get; set; }
    }

    /// <summary>What the tally's changes come to, in the words the night shift reads.</summary>
    public class ShiftTallySummary
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>The open tasks the eMAR's medication pass leaves a resident with.</summary>
    public class EmarTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }
}
