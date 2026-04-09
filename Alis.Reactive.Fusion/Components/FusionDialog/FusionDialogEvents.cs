namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionDialog.
    /// Singleton instance — used with .Reactive() event selector lambda.
    /// </summary>
    public sealed class FusionDialogEvents
    {
        public static readonly FusionDialogEvents Instance = new FusionDialogEvents();
        private FusionDialogEvents() { }

        /// <summary>Fires before the dialog opens (SF "beforeOpen" event).</summary>
        public TypedEvent<FusionDialogBeforeOpenArgs> BeforeOpen =>
            new TypedEvent<FusionDialogBeforeOpenArgs>(
                "beforeOpen", new FusionDialogBeforeOpenArgs());

        /// <summary>Fires before the dialog closes (SF "beforeClose" event).</summary>
        public TypedEvent<FusionDialogBeforeCloseArgs> BeforeClose =>
            new TypedEvent<FusionDialogBeforeCloseArgs>(
                "beforeClose", new FusionDialogBeforeCloseArgs());

        /// <summary>Fires after the dialog is visible (SF "open" event).</summary>
        public TypedEvent<FusionDialogOpenedArgs> Opened =>
            new TypedEvent<FusionDialogOpenedArgs>(
                "open", new FusionDialogOpenedArgs());

        /// <summary>Fires after the dialog is hidden (SF "close" event).</summary>
        public TypedEvent<FusionDialogClosedArgs> Closed =>
            new TypedEvent<FusionDialogClosedArgs>(
                "close", new FusionDialogClosedArgs());

        /// <summary>Fires when the modal overlay is clicked (SF "overlayClick" event).</summary>
        public TypedEvent<FusionDialogOverlayClickArgs> OverlayClick =>
            new TypedEvent<FusionDialogOverlayClickArgs>(
                "overlayClick", new FusionDialogOverlayClickArgs());
    }
}
