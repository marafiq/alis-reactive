namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionDialog.BeforeOpen (SF "beforeOpen" event).
    /// Fires before the dialog opens. Set cancel to true to prevent opening.
    /// </summary>
    public class FusionDialogBeforeOpenArgs
    {
        /// <summary>Set to true to prevent the dialog from opening.</summary>
        public bool Cancel { get; set; }

        public FusionDialogBeforeOpenArgs() { }
    }
}
