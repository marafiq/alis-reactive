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
        /// <summary>Number of files selected.</summary>
        public int FilesCount { get; set; }

        /// <summary>Whether user interaction triggered the selection.</summary>
        public bool IsInteracted { get; set; }
    }
}
