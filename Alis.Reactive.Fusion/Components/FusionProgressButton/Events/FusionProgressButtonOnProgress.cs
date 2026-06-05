namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered by ProgressButton begin, progress, and end events.
    /// </summary>
    public class FusionProgressButtonProgressArgs
    {
        /// <summary>Current progress percentage.</summary>
        public double Percent { get; set; }

        /// <summary>Current progress duration in milliseconds.</summary>
        public double CurrentDuration { get; set; }

        /// <summary>Progress step interval.</summary>
        public double Step { get; set; }
    }
}
