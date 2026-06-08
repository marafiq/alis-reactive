namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionContextMenu"/> component.
    /// </summary>
    public sealed class FusionContextMenuEvents
    {
        public static readonly FusionContextMenuEvents Instance = new FusionContextMenuEvents();

        private FusionContextMenuEvents()
        {
        }

        /// <summary>Fires before each context menu item renders.</summary>
        public TypedEvent<FusionContextMenuBeforeItemRenderArgs> BeforeItemRender =>
            new TypedEvent<FusionContextMenuBeforeItemRenderArgs>("beforeItemRender", new FusionContextMenuBeforeItemRenderArgs());

        /// <summary>Fires before the context menu opens.</summary>
        public TypedEvent<FusionContextMenuBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionContextMenuBeforeOpenArgs>("beforeOpen", new FusionContextMenuBeforeOpenArgs());

        /// <summary>Fires after the context menu opens.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> Opened =>
            new TypedEvent<FusionContextMenuOpenCloseArgs>("onOpen", new FusionContextMenuOpenCloseArgs());

        /// <summary>Fires before the context menu closes.</summary>
        public TypedEvent<FusionContextMenuBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionContextMenuBeforeCloseArgs>("beforeClose", new FusionContextMenuBeforeCloseArgs());

        /// <summary>Fires after the context menu closes.</summary>
        public TypedEvent<FusionContextMenuOpenCloseArgs> Closed =>
            new TypedEvent<FusionContextMenuOpenCloseArgs>("onClose", new FusionContextMenuOpenCloseArgs());

        /// <summary>Fires when a menu item is selected.</summary>
        public TypedEvent<FusionContextMenuSelectArgs> Select =>
            new TypedEvent<FusionContextMenuSelectArgs>("select", new FusionContextMenuSelectArgs());
    }
}
