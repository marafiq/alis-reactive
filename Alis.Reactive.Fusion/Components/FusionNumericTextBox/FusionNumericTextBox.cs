namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion numeric text box component for entering numeric values.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionNumericTextBox&gt;(m =&gt; m.Amount)</c>
    /// to access FusionNumericTextBox-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionNumericTextBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionNumericTextBox(), "numerictextbox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
