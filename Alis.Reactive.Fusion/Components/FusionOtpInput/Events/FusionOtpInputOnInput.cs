namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field changes.
    /// </summary>
    public class FusionOtpInputInputArgs
    {
        /// <summary>Full OTP value after the input event.</summary>
        public string? Value { get; set; }

        /// <summary>Previous OTP value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Zero-based field index that changed.</summary>
        public int Index { get; set; }
    }
}
