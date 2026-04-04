using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionMultiColumnComboBox"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionMultiColumnComboBoxEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionMultiColumnComboBoxChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiColumnComboBoxEvents Instance = new FusionMultiColumnComboBoxEvents();
        private FusionMultiColumnComboBoxEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionMultiColumnComboBoxChangeArgs> Changed =>
            new ReactiveEvent<FusionMultiColumnComboBoxChangeArgs>(
                "change", ChangedContract);
    }
}
