using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionTimePicker.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).NotNull()
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class FusionTimePickerChangeArgs
    {
        /// <summary>New time value.</summary>
        public DateTime? Value { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionTimePickerChangeArgs() { }
    }
}
