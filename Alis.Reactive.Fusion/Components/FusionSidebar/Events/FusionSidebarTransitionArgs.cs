namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before a Sidebar transition.
    /// </summary>
    public sealed class FusionSidebarTransitionArgs
    {
        /// <summary>Whether the transition was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionSidebarTransitionArgs()
        {
        }
    }
}
