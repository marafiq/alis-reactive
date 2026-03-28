namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion MultiSelect for choosing multiple values from a list.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Allergies)</c>
    /// to access MultiSelect-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
