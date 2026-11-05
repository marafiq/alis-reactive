namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.ArrayManager
{
    public sealed class WardDeskModel
    {
    }

    public sealed class WardCensusResponse
    {
        public string Ward { get; set; } = "";

        public object[] Residents { get; set; } = Array.Empty<object>();
    }

    public sealed class WardStaffingQuery
    {
        public int InResidence { get; set; }
    }

    public sealed class WardStaffingPlan
    {
        public string Plan { get; set; } = "";
    }
}
