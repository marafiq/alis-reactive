namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionDateTimePicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionDateTimePickerEvents
    {
        public static readonly FusionDateTimePickerEvents Instance = new FusionDateTimePickerEvents();
        private FusionDateTimePickerEvents() { }

        /// <summary>Fires when the date-time value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDateTimePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDateTimePickerChangeArgs>(
                "change", new FusionDateTimePickerChangeArgs());
    }
}
