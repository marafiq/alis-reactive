namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionRadioButton"/> is selected.
    /// </summary>
    public class FusionRadioButtonChangeArgs
    {
        /// <summary>Selected radio button value from the Syncfusion change event.</summary>
        public string Value { get; set; } = string.Empty;
    }
}
