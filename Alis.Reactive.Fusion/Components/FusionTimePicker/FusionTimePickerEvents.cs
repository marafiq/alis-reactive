namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionTimePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionTimePickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTimePickerEvents Instance = new FusionTimePickerEvents();
        private FusionTimePickerEvents() { }

        /// <summary>Fires when the time value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionTimePickerChangeArgs> Changed =>
            new ReactiveEvent<FusionTimePickerChangeArgs>(
                "change", new FusionTimePickerChangeArgs());
    }
}
