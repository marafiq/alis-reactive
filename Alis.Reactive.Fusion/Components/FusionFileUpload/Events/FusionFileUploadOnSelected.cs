namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when files are selected in a <see cref="FusionFileUpload"/>.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.FilesCount).Gt(0)</c>.
    /// </remarks>
    public class FusionFileUploadSelectedArgs
    {
        /// <summary>Gets or sets the number of files selected.</summary>
        public int FilesCount { get; set; }

        /// <summary>Gets or sets whether the selection was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionFileUploadSelectedArgs() { }
    }
}
