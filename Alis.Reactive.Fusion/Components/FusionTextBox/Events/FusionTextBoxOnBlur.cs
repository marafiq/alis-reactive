namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionTextBox"/> loses focus.
    /// </summary>
    public class FusionTextBoxBlurArgs
    {
        /// <summary>Current text value.</summary>
        public string? Value { get; set; }
    }
}
