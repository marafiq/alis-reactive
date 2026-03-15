using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeDatePicker.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).NotNull()
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// CoercionTypes infers DateTime → "date" for runtime coercion.
    /// </summary>
    public class NativeDatePickerChangeArgs
    {
        /// <summary>The input's date value after the change.</summary>
        public DateTime? Value { get; set; }

        public NativeDatePickerChangeArgs() { }
    }
}
