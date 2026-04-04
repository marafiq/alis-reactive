namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for the Fusion Accordion <c>expanded</c> event.
    /// </summary>
    public class FusionAccordionExpandedArgs
    {
        /// <summary>The zero-based index of the panel that was expanded/collapsed.</summary>
        public int Index { get; set; }

        /// <summary>True if the panel was expanded, false if collapsed.</summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FusionAccordionExpandedArgs"/> class.
        /// </summary>
        internal FusionAccordionExpandedArgs() { }
    }
}
