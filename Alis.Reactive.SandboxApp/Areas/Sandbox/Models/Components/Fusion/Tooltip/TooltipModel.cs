namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the FusionTooltip sandbox demo.
    /// Non-input component — CareLevel is display-only context for the tooltip target.
    /// </summary>
    public sealed class TooltipModel
    {
        public string CareLevel { get; set; } = "Memory Care";
    }
}
