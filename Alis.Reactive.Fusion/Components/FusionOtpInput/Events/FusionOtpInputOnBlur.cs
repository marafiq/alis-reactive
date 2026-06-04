namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field loses focus.
    /// </summary>
    public class FusionOtpInputBlurArgs
    {
        /// <summary>Gets or sets the current OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the zero-based field index that lost focus.</summary>
        public int Index { get; set; }

        /// <summary>Gets or sets whether the blur came from user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionOtpInputBlurArgs() { }
    }
}
