namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionSidebar"/> component.
    /// </summary>
    public sealed class FusionSidebarEvents
    {
        public static readonly FusionSidebarEvents Instance = new FusionSidebarEvents();

        private FusionSidebarEvents()
        {
        }

        /// <summary>Selects the Syncfusion <c>open</c> transition event.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Opened =>
            new TypedEvent<FusionSidebarTransitionArgs>("open", new FusionSidebarTransitionArgs());

        /// <summary>Selects the Syncfusion <c>close</c> transition event.</summary>
        public TypedEvent<FusionSidebarTransitionArgs> Closed =>
            new TypedEvent<FusionSidebarTransitionArgs>("close", new FusionSidebarTransitionArgs());
    }
}
