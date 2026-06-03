namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> loses focus.
    /// </summary>
    public class FusionTextAreaBlurArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Creates a blur event payload.</summary>
        public FusionTextAreaBlurArgs() { }
    }
}
