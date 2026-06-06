namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion checkbox component backed by Syncfusion EJ2 CheckBox.
    /// </summary>
    public sealed class FusionCheckBox : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionCheckBox(), "checkbox");

        /// <inheritdoc />
        public string ValueMember => "checked";
    }
}
