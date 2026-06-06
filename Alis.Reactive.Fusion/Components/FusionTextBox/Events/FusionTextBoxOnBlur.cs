namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> loses focus.
    /// </summary>
    public class FusionTextBoxBlurArgs
    {
        /// <summary>Text value when the input loses focus.</summary>
        public string? Value { get; set; }
    }
}
