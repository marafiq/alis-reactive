using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionSwitch"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionSwitchEvents
    {
        private static readonly CapabilityProperty CheckedEventMember = CapabilityProperty.Named("checked");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionSwitchChangeArgs>(payload =>
            {
                payload.Read(args => args.Checked, CheckedEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionSwitchEvents Instance = new FusionSwitchEvents();
        private FusionSwitchEvents() { }

        /// <summary>Fires when the switch state changes (SF "change" event).</summary>
        public ReactiveEvent<FusionSwitchChangeArgs> Changed =>
            new ReactiveEvent<FusionSwitchChangeArgs>(
                "change", ChangedContract);
    }
}
