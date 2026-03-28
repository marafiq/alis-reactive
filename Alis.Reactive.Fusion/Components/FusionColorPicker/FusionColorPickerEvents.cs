namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionColorPicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionColorPickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionColorPickerEvents Instance = new FusionColorPickerEvents();
        private FusionColorPickerEvents() { }

        /// <summary>Fires when the color value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionColorPickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionColorPickerChangeArgs>(
                "change", new FusionColorPickerChangeArgs());
    }
}
