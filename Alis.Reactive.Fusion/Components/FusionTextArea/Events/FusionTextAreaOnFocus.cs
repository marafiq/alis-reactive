namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> receives focus.
    /// </summary>
    public class FusionTextAreaFocusArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Creates a focus event payload.</summary>
        public FusionTextAreaFocusArgs() { }
    }
}
