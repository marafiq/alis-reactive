namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionOtpInput"/> field receives focus.
    /// </summary>
    public class FusionOtpInputFocusArgs
    {
        /// <summary>Gets or sets the current OTP value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the zero-based field index that received focus.</summary>
        public int Index { get; set; }

        /// <summary>Gets or sets whether the focus came from user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionOtpInputFocusArgs() { }
    }
}
