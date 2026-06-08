namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionMenu"/> component.
    /// </summary>
    public sealed class FusionMenuEvents
    {
        public static readonly FusionMenuEvents Instance = new FusionMenuEvents();

        private FusionMenuEvents()
        {
        }

        /// <summary>Fires before each menu item renders.</summary>
        public TypedEvent<FusionMenuBeforeItemRenderArgs> BeforeItemRender =>
            new TypedEvent<FusionMenuBeforeItemRenderArgs>("beforeItemRender", new FusionMenuBeforeItemRenderArgs());

        /// <summary>Fires before the menu opens.</summary>
        public TypedEvent<FusionMenuBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionMenuBeforeOpenArgs>("beforeOpen", new FusionMenuBeforeOpenArgs());

        /// <summary>Fires after the menu opens.</summary>
        public TypedEvent<FusionMenuOpenCloseArgs> Opened =>
            new TypedEvent<FusionMenuOpenCloseArgs>("onOpen", new FusionMenuOpenCloseArgs());

        /// <summary>Fires before the menu closes.</summary>
        public TypedEvent<FusionMenuBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionMenuBeforeCloseArgs>("beforeClose", new FusionMenuBeforeCloseArgs());

        /// <summary>Fires after the menu closes.</summary>
        public TypedEvent<FusionMenuOpenCloseArgs> Closed =>
            new TypedEvent<FusionMenuOpenCloseArgs>("onClose", new FusionMenuOpenCloseArgs());

        /// <summary>Fires when a menu item is selected.</summary>
        public TypedEvent<FusionMenuSelectArgs> Select =>
            new TypedEvent<FusionMenuSelectArgs>("select", new FusionMenuSelectArgs());
    }
}
