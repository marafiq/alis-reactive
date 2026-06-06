namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionOtpInput"/> component.
    /// </summary>
    public sealed class FusionOtpInputEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionOtpInputEvents Instance = new FusionOtpInputEvents();
        private FusionOtpInputEvents() { }

        /// <summary>Fires as an OTP field changes while editing.</summary>
        public TypedEvent<FusionOtpInputInputArgs> Input =>
            new TypedEvent<FusionOtpInputInputArgs>(
                "input", new FusionOtpInputInputArgs());

        /// <summary>Fires after the complete OTP value changes.</summary>
        public TypedEvent<FusionOtpInputValueChangedArgs> ValueChanged =>
            new TypedEvent<FusionOtpInputValueChangedArgs>(
                "valueChanged", new FusionOtpInputValueChangedArgs());

        /// <summary>Fires when an OTP field receives focus.</summary>
        public TypedEvent<FusionOtpInputFocusArgs> Focus =>
            new TypedEvent<FusionOtpInputFocusArgs>(
                "focus", new FusionOtpInputFocusArgs());

        /// <summary>Fires when an OTP field loses focus.</summary>
        public TypedEvent<FusionOtpInputBlurArgs> Blur =>
            new TypedEvent<FusionOtpInputBlurArgs>(
                "blur", new FusionOtpInputBlurArgs());
    }
}
