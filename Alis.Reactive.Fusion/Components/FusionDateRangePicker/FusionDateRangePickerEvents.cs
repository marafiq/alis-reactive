namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionDateRangePicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionDateRangePickerEvents
    {
        public static readonly FusionDateRangePickerEvents Instance = new FusionDateRangePickerEvents();
        private FusionDateRangePickerEvents() { }

        /// <summary>Fires when the date range value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDateRangePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDateRangePickerChangeArgs>(
                "change", new FusionDateRangePickerChangeArgs());
    }
}
