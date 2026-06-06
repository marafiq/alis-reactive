namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered by ProgressButton begin, progress, and end events.
    /// </summary>
    public class FusionProgressButtonProgressArgs
    {
        /// <summary>Progress percentage reported by the event.</summary>
        public double Percent { get; set; }

        /// <summary>Progress duration reported by the event, in milliseconds.</summary>
        public double CurrentDuration { get; set; }

        /// <summary>Progress step interval.</summary>
        public double Step { get; set; }
    }
}
