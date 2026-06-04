namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Sidebar transition.
    /// </summary>
    public sealed class FusionSidebarTransitionArgs
    {
        /// <summary>Gets or sets whether Syncfusion reports user interaction for this transition.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Initializes a payload marker for Reactive Plan expression binding.
        /// </summary>
    }
}
