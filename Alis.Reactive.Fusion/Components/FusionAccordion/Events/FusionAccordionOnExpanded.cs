namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered after a FusionAccordion panel expands or collapses.
    /// Use properties such as <c>IsExpanded</c> in typed event conditions; the
    /// Reactive Plan reads them from the event payload, for example
    /// <c>evt.isExpanded</c>.
    /// </summary>
    public class FusionAccordionExpandedArgs
    {
        /// <summary>Zero-based index of the panel that expanded or collapsed.</summary>
        public int Index { get; set; }

        /// <summary>True if the panel was expanded, false if collapsed.</summary>
        public bool IsExpanded { get; set; }
    }
}
