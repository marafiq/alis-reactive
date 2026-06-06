namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries the cancel flag for a tooltip open request.
    /// </summary>
    public class FusionTooltipBeforeOpenArgs
    {
        /// <summary>Set to true to prevent the tooltip from opening.</summary>
        public bool Cancel { get; set; }
    }
}
