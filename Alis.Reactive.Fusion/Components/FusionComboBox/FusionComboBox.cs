namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion combo box component for selecting or entering one string value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionComboBox&gt;(m =&gt; m.Resident)</c>
    /// to access ComboBox-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionComboBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionComboBox(), "combobox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
