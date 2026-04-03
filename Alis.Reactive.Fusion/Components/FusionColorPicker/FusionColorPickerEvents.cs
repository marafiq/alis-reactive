namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionColorPicker"/> component.
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
        public ReactiveEvent<FusionColorPickerChangeArgs> Changed =>
            new ReactiveEvent<FusionColorPickerChangeArgs>(
                "change", new FusionColorPickerChangeArgs());
    }
}
