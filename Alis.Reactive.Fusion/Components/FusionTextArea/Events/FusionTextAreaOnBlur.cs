namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> loses focus.
    /// </summary>
    public class FusionTextAreaBlurArgs
    {
        /// <summary>Text value when the textarea loses focus.</summary>
        public string? Value { get; set; }
    }
}
