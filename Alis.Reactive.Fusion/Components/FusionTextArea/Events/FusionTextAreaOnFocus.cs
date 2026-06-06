namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextArea"/> receives focus.
    /// </summary>
    public class FusionTextAreaFocusArgs
    {
        /// <summary>Text value when the textarea receives focus.</summary>
        public string? Value { get; set; }
    }
}
