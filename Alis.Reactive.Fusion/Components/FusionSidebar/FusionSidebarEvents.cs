using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionSidebar"/> component.
    /// </summary>
    public sealed class FusionSidebarEvents
    {
        public static readonly FusionSidebarEvents Instance = new FusionSidebarEvents();

        private FusionSidebarEvents()
        {
        }

        /// <summary>Fires before the sidebar opens.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Opened =>
            new TypedEvent<FusionSidebarTransitionArgs>("open", new FusionSidebarTransitionArgs());

        /// <summary>Fires before the sidebar closes.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Closed =>
            new TypedEvent<FusionSidebarTransitionArgs>("close", new FusionSidebarTransitionArgs());
    }
}
