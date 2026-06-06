namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries the cancel flag before tooltip content renders.
    /// </summary>
    public class FusionTooltipBeforeRenderArgs
    {
        /// <summary>Set to true to prevent the tooltip from rendering.</summary>
        public bool Cancel { get; set; }
    }
}
