namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class TooltipModel
    {
        public string CareLevel { get; set; } = "Memory Care";

        /// <summary>Keeps the care-level explanations from opening on this screen.</summary>
        public bool HideExplanations { get; set; }
    }
}
