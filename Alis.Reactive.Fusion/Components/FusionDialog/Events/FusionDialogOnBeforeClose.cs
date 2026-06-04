namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionDialog.BeforeClose.
    /// Fires before the dialog closes.
    /// </summary>
    public class FusionDialogBeforeCloseArgs
    {
        /// <summary>Set to true to prevent the dialog from closing.</summary>
        public bool Cancel { get; set; }

        /// <summary>True when the user closed the dialog via X button or overlay click.</summary>
        public bool IsInteracted { get; set; }
    }
}
