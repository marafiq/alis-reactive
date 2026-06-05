namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a complete <see cref="FusionOtpInput"/> value changes.
    /// </summary>
    public class FusionOtpInputValueChangedArgs
    {
        /// <summary>New OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Previous committed OTP value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
