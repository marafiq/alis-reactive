using System;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDateTimePicker"/> value changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.
    /// </remarks>
    public class FusionDateTimePickerChangeArgs
    {
        /// <summary>Gets or sets the new date-time value.</summary>
        public DateTime? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal — instances are created by the event descriptor.
        /// </summary>
        public FusionDateTimePickerChangeArgs() { }
    }
}
