using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionMultiSelect"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionMultiSelectEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");
        private static readonly CapabilityProperty TextEventMember = CapabilityProperty.Named("text");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionMultiSelectChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        private static readonly EventContractAuthoring FilteringContract =
            EventPayloadContractAuthoring.Define<FusionMultiSelectFilteringArgs>(payload =>
            {
                payload.Read(args => args.Text, TextEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiSelectEvents Instance = new FusionMultiSelectEvents();
        private FusionMultiSelectEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionMultiSelectChangeArgs> Changed =>
            new ReactiveEvent<FusionMultiSelectChangeArgs>(
                "change", ChangedContract);

        /// <summary>Fires when the user types to filter (SF "filtering" event).</summary>
        public ReactiveEvent<FusionMultiSelectFilteringArgs> Filtering =>
            new ReactiveEvent<FusionMultiSelectFilteringArgs>(
                "filtering", FilteringContract);
    }
}
