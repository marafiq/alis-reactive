namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionTextArea for long-form text entry backed by Syncfusion EJ2 TextArea.
    /// </summary>
    public sealed class FusionTextArea : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionTextArea(), "textarea");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
