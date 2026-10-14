using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Suite 205's turnover on Maple wing: who moves out, who moves in, and whether housekeeping has turned the suite over.
    /// </summary>
    public class SuiteTurnoverViewModel
    {
        public bool HousekeepingSignedOff { get; set; }
        public SuiteTurnoverResident Departure { get; set; } = new();
        public SuiteTurnoverResident Arrival { get; set; } = new();
    }

    /// <summary>A resident on the Maple wing roster.</summary>
    public class SuiteTurnoverResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    /// <summary>The roster's move-ins and move-outs, saved together as the suite's turnover.</summary>
    public class SuiteTurnoverChanges
    {
        public FusionGridBatchChanges<SuiteTurnoverResident> Changes { get; set; } = new();
    }

    /// <summary>What the saved turnover recorded.</summary>
    public class SuiteTurnoverRecorded
    {
        public string Summary { get; set; } = "";
    }
}
