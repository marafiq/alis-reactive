namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionTooltip.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(evt => evt.BeforeOpen, (args, p) => { ... })
    /// </summary>
    public sealed class FusionTooltipEvents
    {
        public static readonly FusionTooltipEvents Instance = new FusionTooltipEvents();
        private FusionTooltipEvents() { }

        /// <summary>Fires before the tooltip opens (SF "beforeOpen" event). Set cancel to prevent opening.</summary>
        public TypedEvent<FusionTooltipBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionTooltipBeforeOpenArgs>(
                "beforeOpen", new FusionTooltipBeforeOpenArgs());

        /// <summary>Fires before the tooltip closes (SF "beforeClose" event). Set cancel to prevent closing.</summary>
        public TypedEvent<FusionTooltipBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionTooltipBeforeCloseArgs>(
                "beforeClose", new FusionTooltipBeforeCloseArgs());

        /// <summary>Fires after the tooltip is visible (SF "open" event).</summary>
        public TypedEvent<FusionTooltipOpenedArgs> Opened =>
            new TypedEvent<FusionTooltipOpenedArgs>(
                "open", new FusionTooltipOpenedArgs());

        /// <summary>Fires after the tooltip is hidden (SF "close" event).</summary>
        public TypedEvent<FusionTooltipClosedArgs> Closed =>
            new TypedEvent<FusionTooltipClosedArgs>(
                "close", new FusionTooltipClosedArgs());

        /// <summary>Fires before tooltip content renders (SF "beforeRender" event). Used for dynamic content.</summary>
        public TypedEvent<FusionTooltipBeforeRenderArgs> BeforeRender =>
            new TypedEvent<FusionTooltipBeforeRenderArgs>(
                "beforeRender", new FusionTooltipBeforeRenderArgs());
    }
}
