using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionDatePicker.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).NotNull()
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// CoercionTypes infers DateTime → "date" for runtime coercion.
    /// </summary>
    public class FusionDatePickerChangeArgs
    {
        /// <summary>New date value.</summary>
        public DateTime? Value { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionDatePickerChangeArgs() { }
    }
}
