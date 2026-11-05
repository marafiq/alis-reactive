namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Sidebar
{
    public sealed class FusionSidebarModel
    {
        public string ResidentName { get; set; } = "Eleanor Whitfield";
    }

    public sealed class FusionSidebarOpenRequest
    {
        public bool OpenedByCoordinator { get; set; }
    }

    public sealed class FusionSidebarOpenResponse
    {
        public string ServicesSummary { get; set; } = "";
        public string OpenedNote { get; set; } = "";
    }

    public sealed class FusionSidebarCloseRequest
    {
        public bool IsOpen { get; set; }
    }

    public sealed class FusionSidebarCloseResponse
    {
        public string ActivityNote { get; set; } = "";
    }
}
