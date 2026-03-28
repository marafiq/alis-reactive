namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionDateTimePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDateTimePickerEvents
    {
        public static readonly FusionDateTimePickerEvents Instance = new FusionDateTimePickerEvents();
        private FusionDateTimePickerEvents() { }

        /// <summary>Fires when the date-time value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionDateTimePickerChangeArgs> Changed =>
            new TypedEventDescriptor<FusionDateTimePickerChangeArgs>(
                "change", new FusionDateTimePickerChangeArgs());
    }
}
