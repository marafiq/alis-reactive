namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Syncfusion Schedule appointment is clicked.
    /// </summary>
    public class FusionScheduleEventClickArgs
    {
        /// <summary>Set to true before the callback returns to prevent the default quick-info popup.</summary>
        public bool Cancel { get; set; }
    }
}
