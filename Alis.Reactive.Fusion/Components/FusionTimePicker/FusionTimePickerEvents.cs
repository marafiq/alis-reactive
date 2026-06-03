namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionTimePicker"/> component.
    /// </summary>
    public sealed class FusionTimePickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTimePickerEvents Instance = new FusionTimePickerEvents();
        private FusionTimePickerEvents() { }

        /// <summary>Fires when the time value changes.</summary>
        public TypedEvent<FusionTimePickerChangeArgs> Changed =>
            new TypedEvent<FusionTimePickerChangeArgs>(
                "change", new FusionTimePickerChangeArgs());
    }
}
