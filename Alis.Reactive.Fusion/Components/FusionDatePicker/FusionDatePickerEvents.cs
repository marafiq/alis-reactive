namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDatePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDatePickerEvents
    {
        public static readonly FusionDatePickerEvents Instance = new FusionDatePickerEvents();
        private FusionDatePickerEvents() { }

        /// <summary>Fires when the date value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDatePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDatePickerChangeArgs>(
                "change", new FusionDatePickerChangeArgs());
    }
}
