using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class SuiteTurnoverViewModel
    {
        public bool HousekeepingSignedOff { get; set; }
        public SuiteTurnoverResident Departure { get; set; } = new();
        public SuiteTurnoverResident Arrival { get; set; } = new();
    }

    public class SuiteTurnoverResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    public class SuiteTurnoverChanges
    {
        public FusionGridBatchChanges<SuiteTurnoverResident> Changes { get; set; } = new();
    }

    public class SuiteTurnoverRecorded
    {
        public string Summary { get; set; } = "";
    }
}
