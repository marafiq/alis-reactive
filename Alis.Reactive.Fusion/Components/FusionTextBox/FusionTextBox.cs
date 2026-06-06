namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion text box component for short text entry.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionTextBox&gt;(m =&gt; m.ResidentName)</c>
    /// to access FusionTextBox-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionTextBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionTextBox(), "textbox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
