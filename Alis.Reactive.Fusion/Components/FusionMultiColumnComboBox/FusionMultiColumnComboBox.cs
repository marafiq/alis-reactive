namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionMultiColumnComboBox for selecting a value with a multi-column dropdown.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility)</c>
    /// to access FusionMultiColumnComboBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionMultiColumnComboBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
