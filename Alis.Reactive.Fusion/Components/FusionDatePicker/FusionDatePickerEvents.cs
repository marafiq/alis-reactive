namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionDatePicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionDatePickerEvents
    {
        public static readonly FusionDatePickerEvents Instance = new FusionDatePickerEvents();
        private FusionDatePickerEvents() { }

        /// <summary>Fires when the date value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDatePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDatePickerChangeArgs>(
                "change", new FusionDatePickerChangeArgs());
    }
}
