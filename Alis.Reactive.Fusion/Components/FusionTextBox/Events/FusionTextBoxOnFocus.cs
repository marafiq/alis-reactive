namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> receives focus.
    /// </summary>
    public class FusionTextBoxFocusArgs
    {
        /// <summary>Gets or sets the current text value.</summary>
        public string? Value { get; set; }

        /// <summary>Creates a focus event payload.</summary>
        public FusionTextBoxFocusArgs() { }
    }
}
