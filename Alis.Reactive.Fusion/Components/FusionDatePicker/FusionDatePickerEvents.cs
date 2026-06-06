namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDatePicker"/> component.
    /// </summary>
    public sealed class FusionDatePickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDatePickerEvents Instance = new FusionDatePickerEvents();
        private FusionDatePickerEvents() { }

        /// <summary>Fires when the date value changes.</summary>
        public TypedEvent<FusionDatePickerChangeArgs> Changed =>
            new TypedEvent<FusionDatePickerChangeArgs>(
                "change", new FusionDatePickerChangeArgs());
    }
}
