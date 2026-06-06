namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Syncfusion masked text input for format-constrained text.
    /// </summary>
    /// <remarks>
    /// Use as a component type in <c>p.Component&lt;FusionInputMask&gt;(m =&gt; m.PhoneNumber)</c>
    /// to write, focus, or read the masked value in a Reactive Plan pipeline.
    /// </remarks>
    public sealed class FusionInputMask : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionInputMask(), "inputmask");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
