using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftTaskTallyModel
    {
        public List<ShiftTallyResident> Residents { get; set; } = new();
    }

    public class ShiftTallyResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public int OpenTasks { get; set; }

        public int PrnDoses { get; set; }
    }

    public class ShiftTallyChanges
    {
        public FusionGridBatchChanges<ShiftTallyResident>? BatchChanges { get; set; }
    }

    public class ShiftTallySummary
    {
        public string Summary { get; set; } = "";
    }

    public class EmarTaskCount
    {
        public int ResidentId { get; set; }
        public int OpenTasks { get; set; }
        public string Summary { get; set; } = "";
    }
}
