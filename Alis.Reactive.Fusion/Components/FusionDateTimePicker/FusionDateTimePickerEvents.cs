namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionDateTimePicker"/> component.
    /// </summary>
    public sealed class FusionDateTimePickerEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionDateTimePickerEvents Instance = new FusionDateTimePickerEvents();
        private FusionDateTimePickerEvents() { }

        /// <summary>Fires when the date-time value changes.</summary>
        public TypedEvent<FusionDateTimePickerChangeArgs> Changed =>
            new TypedEvent<FusionDateTimePickerChangeArgs>(
                "change", new FusionDateTimePickerChangeArgs());
    }
}
