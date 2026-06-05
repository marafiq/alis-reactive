namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> receives focus.
    /// </summary>
    public class FusionTextAreaFocusArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }
    }
}
