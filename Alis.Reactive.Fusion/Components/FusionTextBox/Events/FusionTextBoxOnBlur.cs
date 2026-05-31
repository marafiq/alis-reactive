namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> loses focus.
    /// </summary>
    public class FusionTextBoxBlurArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionTextBoxBlurArgs() { }
    }
}
