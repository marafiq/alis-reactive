namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Breadcrumb item is clicked.
    /// </summary>
    public class FusionBreadcrumbItemClickArgs
    {
        /// <summary>Gets or sets the clicked item metadata from the Syncfusion event.</summary>
        public FusionBreadcrumbItem Item { get; set; } = new FusionBreadcrumbItem();
        public FusionBreadcrumbItemClickArgs()
        {
        }
    }

    /// <summary>
    /// Narrowed breadcrumb item payload proven from Syncfusion Breadcrumb item click behavior.
    /// </summary>
    public sealed class FusionBreadcrumbItem
    {
        /// <summary>Gets or sets the clicked item's text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Gets or sets the clicked item's id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the clicked item's URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Gets or sets the clicked item's icon CSS classes.</summary>
        public string? IconCss { get; set; }

        /// <summary>Gets or sets whether the clicked item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
