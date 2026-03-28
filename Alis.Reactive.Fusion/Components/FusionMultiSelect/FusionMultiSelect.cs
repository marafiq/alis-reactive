namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionMultiSelect for choosing multiple values from a list.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Allergies)</c>
    /// to access FusionMultiSelect-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
