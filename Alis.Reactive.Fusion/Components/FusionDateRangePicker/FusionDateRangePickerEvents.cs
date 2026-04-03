namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionDateRangePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDateRangePickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDateRangePickerEvents Instance = new FusionDateRangePickerEvents();
        private FusionDateRangePickerEvents() { }

        /// <summary>Fires when the date range value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionDateRangePickerChangeArgs> Changed =>
            new ReactiveEvent<FusionDateRangePickerChangeArgs>(
                "change", new FusionDateRangePickerChangeArgs());
    }
}
