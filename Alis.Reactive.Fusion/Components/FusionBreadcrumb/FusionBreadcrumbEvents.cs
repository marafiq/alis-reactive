namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionBreadcrumb"/> component.
    /// </summary>
    public sealed class FusionBreadcrumbEvents
    {
        public static readonly FusionBreadcrumbEvents Instance = new FusionBreadcrumbEvents();

        private FusionBreadcrumbEvents()
        {
        }

        /// <summary>Fires when a breadcrumb item is clicked.</summary>
        public TypedEvent<FusionBreadcrumbItemClickArgs> ItemClick =>
            new TypedEvent<FusionBreadcrumbItemClickArgs>("itemClick", new FusionBreadcrumbItemClickArgs());
    }
}
