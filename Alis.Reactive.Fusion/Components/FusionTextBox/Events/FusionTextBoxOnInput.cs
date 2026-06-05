namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> value changes during input.
    /// </summary>
    public class FusionTextBoxInputArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Previous input text value.</summary>
        public string? PreviousValue { get; set; }
    }
}
