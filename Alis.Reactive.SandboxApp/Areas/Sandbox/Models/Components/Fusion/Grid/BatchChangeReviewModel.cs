using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NightChecksViewModel
    {
    }

    public class NightCheckResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public int ChecksDone { get; set; }
    }

    public class NightCheckChanges
    {
        public FusionGridBatchChanges<NightCheckResident> Changes { get; set; } = new();
    }

    public class NightChecksLogged
    {
        public List<NightCheckResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
