namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> receives focus.
    /// </summary>
    public class FusionTextBoxFocusArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }
    }
}
