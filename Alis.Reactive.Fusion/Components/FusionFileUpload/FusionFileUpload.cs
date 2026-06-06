namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion Uploader configured for form-mode file selection without auto-upload.
    /// </summary>
    /// <remarks>
    /// Use as a component type in <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Documents)</c>
    /// to read selected file metadata for Reactive Plan conditions or gather.
    /// </remarks>
    public sealed class FusionFileUpload : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionFileUpload(), "fileupload");

        /// <inheritdoc />
        public string ValueMember => "filesData";
    }
}
