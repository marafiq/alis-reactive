namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Breadcrumb item is clicked.
    /// </summary>
    public class FusionBreadcrumbItemClickArgs
    {
        /// <summary>Clicked item metadata from the Syncfusion event.</summary>
        public FusionBreadcrumbItem Item { get; set; } = new FusionBreadcrumbItem();
    }

    /// <summary>
    /// Narrowed breadcrumb item payload proven from Syncfusion Breadcrumb item click behavior.
    /// </summary>
    public sealed class FusionBreadcrumbItem
    {
        /// <summary>Clicked item's text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Clicked item's id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Clicked item's URL.</summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>Clicked item's icon CSS classes.</summary>
        public string? IconCss { get; set; }

        /// <summary>Whether the clicked item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
