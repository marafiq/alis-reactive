namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Sidebar transition.
    /// </summary>
    public sealed class FusionSidebarTransitionArgs
    {
        /// <summary>Whether Syncfusion reports user interaction for this transition.</summary>
        public bool IsInteracted { get; set; }
    }
}
