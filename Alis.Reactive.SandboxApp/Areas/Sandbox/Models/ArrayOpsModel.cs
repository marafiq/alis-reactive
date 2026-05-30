namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Model for the ArrayOps sandbox: a multi-select of care activities.</summary>
    public class ArrayOpsModel
    {
        public string[]? SelectedActivities { get; set; }
    }

    /// <summary>HTTP response for the resident roster — the object array the DSL operates on.</summary>
    public class ResidentRosterResponse
    {
        public ResidentRow[] Residents { get; set; } = System.Array.Empty<ResidentRow>();
    }

    /// <summary>A resident element with members the per-element predicates/selectors read.</summary>
    public class ResidentRow
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Age { get; set; }
        public int Balance { get; set; }
    }
}
