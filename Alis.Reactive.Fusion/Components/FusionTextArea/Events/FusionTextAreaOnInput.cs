namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> value changes during input.
    /// </summary>
    public class FusionTextAreaInputArgs
    {
        /// <summary>Text value reported by the input event.</summary>
        public string? Value { get; set; }

        /// <summary>Previous input text value.</summary>
        public string? PreviousValue { get; set; }
    }
}
