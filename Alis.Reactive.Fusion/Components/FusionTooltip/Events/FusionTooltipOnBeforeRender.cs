namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionTooltip.BeforeRender.
    /// Fires before tooltip content renders. Used for dynamic content injection.
    /// Set cancel to true to prevent rendering.
    /// </summary>
    public class FusionTooltipBeforeRenderArgs
    {
        /// <summary>Set to true to prevent the tooltip from rendering.</summary>
        public bool Cancel { get; set; }

        public FusionTooltipBeforeRenderArgs() { }
    }
}
