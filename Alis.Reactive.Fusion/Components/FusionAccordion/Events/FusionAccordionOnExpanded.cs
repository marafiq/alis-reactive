namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionAccordion.Expanded (SF "expanded" event).
    /// Fires after a panel expands or collapses.
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.IsExpanded).Truthy()
    /// ExpressionPathHelper resolves x => x.IsExpanded to "evt.isExpanded".
    /// </summary>
    public class FusionAccordionExpandedArgs
    {
        /// <summary>The zero-based index of the panel that was expanded/collapsed.</summary>
        public int Index { get; set; }

        /// <summary>True if the panel was expanded, false if collapsed.</summary>
        public bool IsExpanded { get; set; }

        public FusionAccordionExpandedArgs() { }
    }
}
