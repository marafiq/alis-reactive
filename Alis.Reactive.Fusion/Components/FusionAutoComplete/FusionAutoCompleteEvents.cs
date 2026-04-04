using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionAutoComplete"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionAutoCompleteEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");
        private static readonly CapabilityProperty TextEventMember = CapabilityProperty.Named("text");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionAutoCompleteChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        private static readonly EventContractAuthoring FilteringContract =
            EventPayloadContractAuthoring.Define<FusionAutoCompleteFilteringArgs>(payload =>
            {
                payload.Read(args => args.Text, TextEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionAutoCompleteEvents Instance = new FusionAutoCompleteEvents();
        private FusionAutoCompleteEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionAutoCompleteChangeArgs> Changed =>
            new ReactiveEvent<FusionAutoCompleteChangeArgs>(
                "change", ChangedContract);

        /// <summary>Fires when the user types to filter (SF "filtering" event).</summary>
        public ReactiveEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            new ReactiveEvent<FusionAutoCompleteFilteringArgs>(
                "filtering", FilteringContract);
    }
}
