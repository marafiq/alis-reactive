namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries the cancel flag for a tooltip close request.
    /// </summary>
    public class FusionTooltipBeforeCloseArgs
    {
        /// <summary>Set to true to prevent the tooltip from closing.</summary>
        public bool Cancel { get; set; }
    }
}
