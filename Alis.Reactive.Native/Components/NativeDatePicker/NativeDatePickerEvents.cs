namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeDatePicker.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeDatePickerEvents
    {
        public static readonly NativeDatePickerEvents Instance = new NativeDatePickerEvents();
        private NativeDatePickerEvents() { }

        /// <summary>Fires when the user changes the date value (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeDatePickerChangeArgs> Changed =>
            new TypedEventDescriptor<NativeDatePickerChangeArgs>(
                "change", new NativeDatePickerChangeArgs());
    }
}
