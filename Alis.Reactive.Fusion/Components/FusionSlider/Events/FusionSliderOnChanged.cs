namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a scalar <see cref="FusionSlider"/> value changes.
    /// </summary>
    public class FusionSliderChangeArgs
    {
        /// <summary>Gets or sets the new slider value.</summary>
        public double Value { get; set; }

        /// <summary>Gets or sets the previous slider value.</summary>
        public double PreviousValue { get; set; }

        /// <summary>Gets or sets the formatted slider value text.</summary>
        public string? Text { get; set; }

        /// <summary>Gets or sets the Syncfusion action name for the change.</summary>
        public string? Action { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }
        public FusionSliderChangeArgs() { }
    }
}
