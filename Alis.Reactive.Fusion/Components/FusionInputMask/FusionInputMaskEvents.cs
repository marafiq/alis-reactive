using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionInputMask"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionInputMaskEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionInputMaskChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionInputMaskEvents Instance = new FusionInputMaskEvents();
        private FusionInputMaskEvents() { }

        /// <summary>Fires when the masked value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionInputMaskChangeArgs> Changed =>
            new ReactiveEvent<FusionInputMaskChangeArgs>(
                "change", ChangedContract);
    }
}
