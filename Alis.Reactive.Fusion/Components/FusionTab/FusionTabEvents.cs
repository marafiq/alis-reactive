using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Fusion Tab component.
    /// </summary>
    public sealed class FusionTabEvents
    {
        private static readonly CapabilityProperty SelectedIndexEventMember = CapabilityProperty.Named("selectedIndex");
        private static readonly CapabilityProperty PreviousIndexEventMember = CapabilityProperty.Named("previousIndex");
        private static readonly CapabilityProperty IsSwipedEventMember = CapabilityProperty.Named("isSwiped");

        private static readonly EventContractAuthoring SelectedContract =
            EventPayloadContractAuthoring.Define<FusionTabSelectedArgs>(payload =>
            {
                payload.Read(args => args.SelectedIndex, SelectedIndexEventMember);
                payload.Read(args => args.PreviousIndex, PreviousIndexEventMember);
                payload.Read(args => args.IsSwiped, IsSwipedEventMember);
            });

        /// <summary>
        /// Gets the singleton event catalog for Fusion Tab.
        /// </summary>
        public static readonly FusionTabEvents Instance = new FusionTabEvents();
        private FusionTabEvents() { }

        /// <summary>Fires when a tab is selected (SF "selected" event).</summary>
        public ReactiveEvent<FusionTabSelectedArgs> Selected =>
            new ReactiveEvent<FusionTabSelectedArgs>(
                "selected", SelectedContract);
    }
}
