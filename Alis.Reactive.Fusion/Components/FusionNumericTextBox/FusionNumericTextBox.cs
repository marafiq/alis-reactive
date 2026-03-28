namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionNumericTextBox for entering and validating numeric values.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Amount)</c>
    /// to access FusionNumericTextBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
