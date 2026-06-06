namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionFileUpload for selecting files in form mode (no auto-upload).
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Documents)</c>
    /// to access FusionFileUpload-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionFileUpload : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionFileUpload(), "fileupload");

        /// <inheritdoc />
        public string ValueMember => "filesData";
    }
}
