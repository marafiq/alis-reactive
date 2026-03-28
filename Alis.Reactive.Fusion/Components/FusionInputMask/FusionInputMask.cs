namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A Syncfusion MaskedTextBox for entering text with a format mask (e.g. phone numbers).
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionInputMask&gt;(m =&gt; m.PhoneNumber)</c>
    /// to access InputMask-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionInputMask : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ReadExpr => "value";
    }
}
