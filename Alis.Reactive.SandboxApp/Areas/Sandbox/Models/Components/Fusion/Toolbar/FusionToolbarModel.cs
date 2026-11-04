namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Toolbar
{
    public sealed class FusionToolbarModel
    {
        public string ResidentName { get; set; } = "Margaret Lewis";

        public decimal BalanceDue { get; set; } = 248.50m;
    }

    public sealed class ResidentCommandRequest
    {
        public string CommandId { get; set; } = string.Empty;

        public string CommandText { get; set; } = string.Empty;

        public bool CommandDisabled { get; set; }
    }

    public sealed class ResidentCommandResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
