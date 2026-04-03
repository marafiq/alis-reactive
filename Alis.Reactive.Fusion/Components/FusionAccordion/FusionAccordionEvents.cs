namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Fusion Accordion component.
    /// </summary>
    public sealed class FusionAccordionEvents
    {
        /// <summary>
        /// Gets the singleton event catalog for Fusion Accordion.
        /// </summary>
        public static readonly FusionAccordionEvents Instance = new FusionAccordionEvents();
        private FusionAccordionEvents() { }

        /// <summary>Fires after a panel expands or collapses (SF "expanded" event).</summary>
        public ReactiveEvent<FusionAccordionExpandedArgs> Expanded =>
            new ReactiveEvent<FusionAccordionExpandedArgs>(
                "expanded", new FusionAccordionExpandedArgs());
    }
}
