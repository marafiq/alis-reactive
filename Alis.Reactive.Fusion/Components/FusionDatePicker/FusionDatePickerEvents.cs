namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionDatePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDatePickerEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDatePickerEvents Instance = new FusionDatePickerEvents();
        private FusionDatePickerEvents() { }

        /// <summary>Fires when the date value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionDatePickerChangeArgs> Changed =>
            new ReactiveEvent<FusionDatePickerChangeArgs>(
                "change", new FusionDatePickerChangeArgs());
    }
}
