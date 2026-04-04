using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionDateRangePicker"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDateRangePickerEvents
    {
        private static readonly CapabilityProperty StartDateEventMember = CapabilityProperty.Named("startDate");
        private static readonly CapabilityProperty EndDateEventMember = CapabilityProperty.Named("endDate");
        private static readonly CapabilityProperty DaySpanEventMember = CapabilityProperty.Named("daySpan");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionDateRangePickerChangeArgs>(payload =>
            {
                payload.Read(args => args.StartDate, StartDateEventMember);
                payload.Read(args => args.EndDate, EndDateEventMember);
                payload.Read(args => args.DaySpan, DaySpanEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDateRangePickerEvents Instance = new FusionDateRangePickerEvents();
        private FusionDateRangePickerEvents() { }

        /// <summary>Fires when the date range value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionDateRangePickerChangeArgs> Changed =>
            new ReactiveEvent<FusionDateRangePickerChangeArgs>(
                "change", ChangedContract);
    }
}
