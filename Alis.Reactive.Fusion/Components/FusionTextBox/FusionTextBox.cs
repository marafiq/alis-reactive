namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionTextBox for short text entry backed by Syncfusion EJ2 TextBox.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionTextBox&gt;(m =&gt; m.ResidentName)</c>
    /// to access FusionTextBox-specific mutations and value reading.
    /// </remarks>
    public sealed class FusionTextBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionTextBox(), "textbox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
