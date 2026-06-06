namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionColorPicker"/> component.
    /// </summary>
    public sealed class FusionColorPickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionColorPickerEvents Instance = new FusionColorPickerEvents();
        private FusionColorPickerEvents() { }

        /// <summary>Fires when the color value changes.</summary>
        public TypedEvent<FusionColorPickerChangeArgs> Changed =>
            new TypedEvent<FusionColorPickerChangeArgs>(
                "change", new FusionColorPickerChangeArgs());
    }
}
