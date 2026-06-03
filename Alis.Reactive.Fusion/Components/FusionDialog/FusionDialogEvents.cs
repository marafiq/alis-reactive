namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDialog"/> component.
    /// </summary>
    public sealed class FusionDialogEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDialogEvents Instance = new FusionDialogEvents();
        private FusionDialogEvents() { }

        /// <summary>Fires before the dialog opens.</summary>
        public TypedEvent<FusionDialogBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionDialogBeforeOpenArgs>(
                "beforeOpen", new FusionDialogBeforeOpenArgs());

        /// <summary>Fires before the dialog closes.</summary>
        public TypedEvent<FusionDialogBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionDialogBeforeCloseArgs>(
                "beforeClose", new FusionDialogBeforeCloseArgs());

        /// <summary>Fires after the dialog is visible.</summary>
        public TypedEvent<FusionDialogOpenedArgs> Opened =>
            new TypedEvent<FusionDialogOpenedArgs>(
                "open", new FusionDialogOpenedArgs());

        /// <summary>Fires after the dialog is hidden.</summary>
        public TypedEvent<FusionDialogClosedArgs> Closed =>
            new TypedEvent<FusionDialogClosedArgs>(
                "close", new FusionDialogClosedArgs());

        /// <summary>Fires when the modal overlay is clicked.</summary>
        public TypedEvent<FusionDialogOverlayClickArgs> OverlayClick =>
            new TypedEvent<FusionDialogOverlayClickArgs>(
                "overlayClick", new FusionDialogOverlayClickArgs());
    }
}
