namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// A FusionOtpInput for entering a one-time passcode value.
    /// </summary>
    public sealed class FusionOtpInput : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionOtpInput(), "otpinput");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
