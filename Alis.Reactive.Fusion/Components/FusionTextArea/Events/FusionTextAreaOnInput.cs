namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> value changes during input.
    /// </summary>
    public class FusionTextAreaInputArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the previously input text value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Creates an input event payload.</summary>
        public FusionTextAreaInputArgs() { }
    }
}
