using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The night safety checks page: the checks load from the night log, so the page has no fields of its own.</summary>
    public class NightChecksViewModel
    {
    }

    /// <summary>A resident on Cedar wing's two-hourly safety checks, with the checks recorded tonight.</summary>
    public class NightCheckResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public int ChecksDone { get; set; }
    }

    /// <summary>The checks the charge nurse changed, saved to the night log once she confirms them.</summary>
    public class NightCheckChanges
    {
        public FusionGridBatchChanges<NightCheckResident> Changes { get; set; } = new();
    }

    /// <summary>The checks as the night log now holds them, and what was saved.</summary>
    public class NightChecksLogged
    {
        public List<NightCheckResident> Residents { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
