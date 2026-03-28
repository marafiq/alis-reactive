namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDateRangePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
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
