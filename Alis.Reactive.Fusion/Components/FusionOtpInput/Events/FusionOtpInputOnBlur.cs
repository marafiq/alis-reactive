namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field loses focus.
    /// </summary>
    public class FusionOtpInputBlurArgs
    {
        /// <summary>Current OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Zero-based field index that lost focus.</summary>
        public int Index { get; set; }

        /// <summary>Whether user interaction caused the blur.</summary>
        public bool IsInteracted { get; set; }
    }
}
