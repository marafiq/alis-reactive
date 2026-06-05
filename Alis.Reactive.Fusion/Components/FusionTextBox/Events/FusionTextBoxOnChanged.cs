namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> value changes and focus leaves the input.
    /// </summary>
    public class FusionTextBoxChangeArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Previous committed text value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
