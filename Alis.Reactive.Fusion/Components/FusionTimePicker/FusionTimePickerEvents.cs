namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionTimePicker"/> component.
    /// </summary>
    public sealed class FusionTimePickerEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionTimePickerEvents Instance = new FusionTimePickerEvents();
        private FusionTimePickerEvents() { }

        /// <summary>Fires when the time value changes.</summary>
        public TypedEvent<FusionTimePickerChangeArgs> Changed =>
            new TypedEvent<FusionTimePickerChangeArgs>(
                "change", new FusionTimePickerChangeArgs());
    }
}
