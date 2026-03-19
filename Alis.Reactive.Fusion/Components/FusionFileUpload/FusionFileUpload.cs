namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion Uploader component in form mode (no auto-upload).
    /// Phantom type — zero state. Used as type parameter in
    /// p.Component&lt;FusionFileUpload&gt;(m => m.Documents) to unlock
    /// the FileUpload-specific extension methods.
    /// </summary>
    public sealed class FusionFileUpload : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "element.files";
    }
}
