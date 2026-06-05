namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> loses focus.
    /// </summary>
    public class FusionTextAreaBlurArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }
    }
}
