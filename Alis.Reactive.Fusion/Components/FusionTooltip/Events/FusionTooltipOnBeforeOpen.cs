namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionTooltip.BeforeOpen (SF "beforeOpen" event).
    /// Fires before the tooltip opens. Set cancel to true to prevent opening.
    /// </summary>
    public class FusionTooltipBeforeOpenArgs
    {
        /// <summary>Set to true to prevent the tooltip from opening.</summary>
        public bool Cancel { get; set; }

        public FusionTooltipBeforeOpenArgs() { }
    }
}
