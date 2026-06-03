namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered by ProgressButton begin, progress, and end events.
    /// </summary>
    public class FusionProgressButtonProgressArgs
    {
        /// <summary>Gets or sets the current progress percentage.</summary>
        public double Percent { get; set; }

        /// <summary>Gets or sets the current progress duration in milliseconds.</summary>
        public double CurrentDuration { get; set; }

        /// <summary>Gets or sets the progress step interval.</summary>
        public double Step { get; set; }

        /// <summary>
        /// Creates an event payload instance for descriptor wiring.
        /// </summary>
        public FusionProgressButtonProgressArgs() { }
    }
}
