namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> receives focus.
    /// </summary>
    public class FusionTextBoxFocusArgs
    {
        /// <summary>Text value when the input receives focus.</summary>
        public string? Value { get; set; }
    }
}
