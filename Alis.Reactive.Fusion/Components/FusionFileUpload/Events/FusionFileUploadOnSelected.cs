namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionFileUpload.Selected (SF "selected" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.FilesCount).GreaterThan(0)
    /// ExpressionPathHelper resolves x => x.FilesCount to "evt.filesCount".
    /// </summary>
    public class FusionFileUploadSelectedArgs
    {
        /// <summary>The number of files selected.</summary>
        public int FilesCount { get; set; }

        /// <summary>True if the selection was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionFileUploadSelectedArgs() { }
    }
}
