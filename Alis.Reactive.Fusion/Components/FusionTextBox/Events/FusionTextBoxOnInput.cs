namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> value changes during input.
    /// </summary>
    public class FusionTextBoxInputArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the previously input text value.</summary>
        public string? PreviousValue { get; set; }
    }
}
