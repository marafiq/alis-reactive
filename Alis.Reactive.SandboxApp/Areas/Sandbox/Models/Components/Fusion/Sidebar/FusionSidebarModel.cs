namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Sidebar
{
    /// <summary>
    /// Resident Care Dashboard view model. The dashboard names the resident whose
    /// care the coordinator is reviewing; the care-services navigation panel slides
    /// in from the side.
    /// </summary>
    public sealed class FusionSidebarModel
    {
        public string ResidentName { get; set; } = "Eleanor Whitfield";
    }

    /// <summary>
    /// Request posted when the care-services panel opens. <see cref="OpenedByCoordinator"/>
    /// is the sidebar transition's IsInteracted flag — whether a person opened it.
    /// </summary>
    public sealed class FusionSidebarOpenRequest
    {
        public bool OpenedByCoordinator { get; set; }
    }

    /// <summary>Live care-services summary the server returns when the panel opens.</summary>
    public sealed class FusionSidebarOpenResponse
    {
        public string ServicesSummary { get; set; } = "";
        public string OpenedNote { get; set; } = "";
    }

    /// <summary>
    /// Request posted when the panel is closed by the toolbar button. <see cref="IsOpen"/>
    /// is the sidebar's IsOpen() read after Hide() — proving the panel is shut.
    /// </summary>
    public sealed class FusionSidebarCloseRequest
    {
        public bool IsOpen { get; set; }
    }

    /// <summary>Confirmation the server returns after the panel is closed.</summary>
    public sealed class FusionSidebarCloseResponse
    {
        public string ActivityNote { get; set; } = "";
    }
}
