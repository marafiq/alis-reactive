namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionTooltip"/> component.
    /// </summary>
    public sealed class FusionTooltipEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTooltipEvents Instance = new FusionTooltipEvents();
        private FusionTooltipEvents() { }

        /// <summary>Fires before the tooltip opens. Set cancel to prevent opening.</summary>
        public TypedEvent<FusionTooltipBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionTooltipBeforeOpenArgs>(
                "beforeOpen", new FusionTooltipBeforeOpenArgs());

        /// <summary>Fires before the tooltip closes. Set cancel to prevent closing.</summary>
        public TypedEvent<FusionTooltipBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionTooltipBeforeCloseArgs>(
                "beforeClose", new FusionTooltipBeforeCloseArgs());

        /// <summary>Fires after the tooltip is visible.</summary>
        public TypedEvent<FusionTooltipOpenedArgs> Opened =>
            new TypedEvent<FusionTooltipOpenedArgs>(
                "open", new FusionTooltipOpenedArgs());

        /// <summary>Fires after the tooltip is hidden.</summary>
        public TypedEvent<FusionTooltipClosedArgs> Closed =>
            new TypedEvent<FusionTooltipClosedArgs>(
                "close", new FusionTooltipClosedArgs());

        /// <summary>Fires before tooltip content renders. Used for dynamic content.</summary>
        public TypedEvent<FusionTooltipBeforeRenderArgs> BeforeRender =>
            new TypedEvent<FusionTooltipBeforeRenderArgs>(
                "beforeRender", new FusionTooltipBeforeRenderArgs());
    }
}
