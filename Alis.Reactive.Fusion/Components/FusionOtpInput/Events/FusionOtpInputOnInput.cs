namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field changes.
    /// </summary>
    public class FusionOtpInputInputArgs
    {
        /// <summary>Gets or sets the current OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the previous OTP value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Gets or sets the zero-based field index that changed.</summary>
        public int Index { get; set; }
    }
}
