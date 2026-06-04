namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Sidebar
{
    public sealed class FusionSidebarModel
    {
    }

    public sealed class FusionSidebarOpenRequest
    {
        public bool IsInteracted { get; set; }
    }

    public sealed class FusionSidebarOpenResponse
    {
        public string Message { get; set; } = "";
        public string PanelTitle { get; set; } = "";
        public string OpenMode { get; set; } = "";
    }
}
