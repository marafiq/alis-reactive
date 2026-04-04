using System;
using Alis.Reactive.PlanModel;

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
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionTimePickerChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTimePickerEvents Instance = new FusionTimePickerEvents();
        private FusionTimePickerEvents() { }

        /// <summary>Fires when the time value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionTimePickerChangeArgs> Changed =>
            new ReactiveEvent<FusionTimePickerChangeArgs>(
                "change", ChangedContract);
    }
}
