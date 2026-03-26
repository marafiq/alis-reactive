namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionAccordion.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(evt => evt.Expanded, (args, p) => { ... })
    /// </summary>
    public sealed class FusionAccordionEvents
    {
        public static readonly FusionAccordionEvents Instance = new FusionAccordionEvents();
        private FusionAccordionEvents() { }

        /// <summary>Fires after a panel expands or collapses (SF "expanded" event).</summary>
        public TypedEventDescriptor<FusionAccordionExpandedArgs> Expanded =>
            new TypedEventDescriptor<FusionAccordionExpandedArgs>(
                "expanded", new FusionAccordionExpandedArgs());
    }
}
