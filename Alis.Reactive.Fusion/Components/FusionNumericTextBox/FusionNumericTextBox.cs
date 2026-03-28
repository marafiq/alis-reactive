namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion NumericTextBox for entering and validating numeric values.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Amount)</c>
    /// to access NumericTextBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
