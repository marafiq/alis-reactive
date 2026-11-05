using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TaskCountViewModel
    {
        public List<TaskCountRow> Residents { get; set; } = new();
    }

    public class TaskCountRow
    {
        public int Room { get; set; }
        public string ResidentName { get; set; } = "";
        public int OpenTasks { get; set; }
        public int MissedDoses { get; set; }
    }

    public class TaskCountChanges
    {
        public FusionGridBatchChanges<TaskCountRow> Changes { get; set; } = new();
    }

    public class TaskCountSummary
    {
        public string Summary { get; set; } = "";
    }

    public class TaskBoardCount
    {
        public int RowIndex { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }
}
