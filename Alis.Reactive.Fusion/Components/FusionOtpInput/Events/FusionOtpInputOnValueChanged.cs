namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a complete <see cref="FusionOtpInput"/> value changes.
    /// </summary>
    public class FusionOtpInputValueChangedArgs
    {
        /// <summary>Gets or sets the new OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the previous committed OTP value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionOtpInputValueChangedArgs() { }
    }
}
