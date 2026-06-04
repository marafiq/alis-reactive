namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> value changes and focus leaves the input.
    /// </summary>
    public class FusionTextAreaChangeArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets the previous committed text value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionTextAreaChangeArgs() { }
    }
}
