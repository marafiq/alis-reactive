using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The evening task count: one row per resident on the wing.</summary>
    public class TaskCountViewModel
    {
        public List<TaskCountRow> Residents { get; set; } = new();
    }

    /// <summary>A resident's counts at the end of the evening shift.</summary>
    public class TaskCountRow
    {
        public int Room { get; set; }
        public string ResidentName { get; set; } = "";
        public int OpenTasks { get; set; }
        public int MissedDoses { get; set; }
    }

    /// <summary>The counts changed in the sheet and not yet saved.</summary>
    public class TaskCountChanges
    {
        public FusionGridBatchChanges<TaskCountRow> Changes { get; set; } = new();
    }

    public class TaskCountSummary
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>A count the task board has that the sheet does not: the row, and the open tasks on the board.</summary>
    public class TaskBoardCount
    {
        public int RowIndex { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }
}
