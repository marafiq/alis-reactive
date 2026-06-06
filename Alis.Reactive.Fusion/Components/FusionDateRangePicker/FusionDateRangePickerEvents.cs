namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDateRangePicker"/> component.
    /// </summary>
    public sealed class FusionDateRangePickerEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionDateRangePickerEvents Instance = new FusionDateRangePickerEvents();
        private FusionDateRangePickerEvents() { }

        /// <summary>Fires when the date range value changes.</summary>
        public TypedEvent<FusionDateRangePickerChangeArgs> Changed =>
            new TypedEvent<FusionDateRangePickerChangeArgs>(
                "change", new FusionDateRangePickerChangeArgs());
    }
}
