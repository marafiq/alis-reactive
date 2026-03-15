namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeDatePicker.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("2026-01-01")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeDatePickerChangeArgs
    {
        /// <summary>The input's value after the change (ISO 8601 date string).</summary>
        public string? Value { get; set; }

        public NativeDatePickerChangeArgs() { }
    }
}
