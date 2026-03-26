namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionColorPicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionColorPickerEvents
    {
        public static readonly FusionColorPickerEvents Instance = new FusionColorPickerEvents();
        private FusionColorPickerEvents() { }

        /// <summary>Fires when the color value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionColorPickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionColorPickerChangeArgs>(
                "change", new FusionColorPickerChangeArgs());
    }
}
