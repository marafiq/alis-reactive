namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a scalar <see cref="FusionSlider"/> value changes.
    /// </summary>
    public class FusionSliderChangeArgs
    {
        /// <summary>New slider value.</summary>
        public double Value { get; set; }

        /// <summary>Previous slider value.</summary>
        public double PreviousValue { get; set; }

        /// <summary>Formatted slider value text.</summary>
        public string? Text { get; set; }

        /// <summary>Syncfusion action name for the change.</summary>
        public string? Action { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
