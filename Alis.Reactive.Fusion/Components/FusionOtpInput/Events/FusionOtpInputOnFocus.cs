namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field receives focus.
    /// </summary>
    public class FusionOtpInputFocusArgs
    {
        /// <summary>Current OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Zero-based field index that received focus.</summary>
        public int Index { get; set; }

        /// <summary>Whether user interaction caused the focus.</summary>
        public bool IsInteracted { get; set; }
    }
}
