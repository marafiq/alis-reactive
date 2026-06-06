namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionAccordion"/> component.
    /// </summary>
    public sealed class FusionAccordionEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionAccordionEvents Instance = new FusionAccordionEvents();
        private FusionAccordionEvents() { }

        /// <summary>Fires after a panel expands or collapses.</summary>
        public TypedEvent<FusionAccordionExpandedArgs> Expanded =>
            new TypedEvent<FusionAccordionExpandedArgs>(
                "expanded", new FusionAccordionExpandedArgs());
    }
}
