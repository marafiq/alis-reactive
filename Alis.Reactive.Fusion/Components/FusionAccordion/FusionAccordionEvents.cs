using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Fusion Accordion component.
    /// </summary>
    public sealed class FusionAccordionEvents
    {
        private static readonly CapabilityProperty IndexEventMember = CapabilityProperty.Named("index");
        private static readonly CapabilityProperty IsExpandedEventMember = CapabilityProperty.Named("isExpanded");

        private static readonly EventContractAuthoring ExpandedContract =
            EventPayloadContractAuthoring.Define<FusionAccordionExpandedArgs>(payload =>
            {
                payload.Read(args => args.Index, IndexEventMember);
                payload.Read(args => args.IsExpanded, IsExpandedEventMember);
            });

        /// <summary>
        /// Gets the singleton event catalog for Fusion Accordion.
        /// </summary>
        public static readonly FusionAccordionEvents Instance = new FusionAccordionEvents();
        private FusionAccordionEvents() { }

        /// <summary>Fires after a panel expands or collapses (SF "expanded" event).</summary>
        public ReactiveEvent<FusionAccordionExpandedArgs> Expanded =>
            new ReactiveEvent<FusionAccordionExpandedArgs>(
                "expanded", ExpandedContract);
    }
}
