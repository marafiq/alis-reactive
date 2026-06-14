namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Toolbar
{
    /// <summary>
    /// View model for the resident account command bar. The resident's name and
    /// current balance frame the command bar that runs their account actions.
    /// </summary>
    public sealed class FusionToolbarModel
    {
        public string ResidentName { get; set; } = "Margaret Lewis";

        public decimal BalanceDue { get; set; } = 248.50m;
    }

    /// <summary>
    /// The clicked command the resident sends when paying their balance.
    /// Carries the toolbar item's typed click-payload fields.
    /// </summary>
    public sealed class ResidentCommandRequest
    {
        public string CommandId { get; set; } = string.Empty;

        public string CommandText { get; set; } = string.Empty;

        public bool CommandDisabled { get; set; }
    }

    /// <summary>
    /// The server's confirmation that the resident's payment command was received.
    /// </summary>
    public sealed class ResidentCommandResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
