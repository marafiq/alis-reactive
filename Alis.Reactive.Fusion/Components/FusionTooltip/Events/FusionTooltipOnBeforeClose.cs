namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionTooltip.BeforeClose.
    /// Fires before the tooltip closes. Set cancel to true to prevent closing.
    /// </summary>
    public class FusionTooltipBeforeCloseArgs
    {
        /// <summary>Set to true to prevent the tooltip from closing.</summary>
        public bool Cancel { get; set; }

        public FusionTooltipBeforeCloseArgs() { }
    }
}
