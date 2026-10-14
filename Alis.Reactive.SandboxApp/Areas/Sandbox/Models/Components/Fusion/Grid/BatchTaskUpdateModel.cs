using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The day task board page: the board loads its residents from the record, so the page has no fields of its own.</summary>
    public class DayTaskBoardViewModel
    {
    }

    /// <summary>A resident on Birch wing's day task board, with the open tasks the day shift is carrying.</summary>
    public class DayTaskBoardResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public int OpenTasks { get; set; }
    }

    /// <summary>The board's changes, handed over to the evening shift.</summary>
    public class DayTaskBoardChanges
    {
        public FusionGridBatchChanges<DayTaskBoardResident> Changes { get; set; } = new();
    }

    /// <summary>The board as the handover saved it, and what changed against the record.</summary>
    public class DayTaskBoardHandedOver
    {
        public List<DayTaskBoardResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
