namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionComboBox for selecting or entering a single string value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionComboBox&gt;(m =&gt; m.Resident)</c>
    /// to access ComboBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionComboBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionComboBox(), "combobox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
