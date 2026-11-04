using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class DayTaskBoardViewModel
    {
    }

    public class DayTaskBoardResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public int OpenTasks { get; set; }
    }

    public class DayTaskBoardChanges
    {
        public FusionGridBatchChanges<DayTaskBoardResident> Changes { get; set; } = new();
    }

    public class DayTaskBoardHandedOver
    {
        public List<DayTaskBoardResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
