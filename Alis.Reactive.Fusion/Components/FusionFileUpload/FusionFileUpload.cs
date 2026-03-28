namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionFileUpload for selecting files in form mode (no auto-upload).
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionFileUpload&gt;(m =&gt; m.Documents)</c>
    /// to access FusionFileUpload-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionFileUpload : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "filesData";
    }
}
