namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: an admissions coordinator releases a room held for an incoming resident. The community
    // asks before a hold is released in its own confirmation dialog, which the page brings in place of
    // the framework's.
    public class HoldReleaseModel
    {
        public string Room { get; set; } = "214";
    }
}
