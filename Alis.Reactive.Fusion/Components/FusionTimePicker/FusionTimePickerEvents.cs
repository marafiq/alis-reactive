namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionTimePicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionTimePickerEvents
    {
        public static readonly FusionTimePickerEvents Instance = new FusionTimePickerEvents();
        private FusionTimePickerEvents() { }

        /// <summary>Fires when the time value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionTimePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionTimePickerChangeArgs>(
                "change", new FusionTimePickerChangeArgs());
    }
}
