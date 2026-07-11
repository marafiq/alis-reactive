namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The resident whose care record the coordinator is navigating. Drives the
    /// breadcrumb trail (community -> resident -> section) rendered by the view.
    /// </summary>
    public sealed class FusionBreadcrumbModel
    {
        public string CommunityName { get; set; } = "Sunrise Court";

        public string ResidentName { get; set; } = "Eleanor Hughes";
    }

    /// <summary>
    /// The crumb a coordinator clicked. The view gathers the clicked item's
    /// identity (text, id, url, disabled) into this request to open that section.
    /// </summary>
    public sealed class OpenCareSectionRequest
    {
        public string Text { get; set; } = "";

        public string Id { get; set; } = "";

        public string Url { get; set; } = "";

        public bool Disabled { get; set; }
    }

    /// <summary>
    /// The section the server opened for the clicked crumb. The coordinator sees
    /// the heading, the summary, and the section code resolved from the crumb's
    /// url and id.
    /// </summary>
    public sealed class OpenCareSectionResponse
    {
        public string Heading { get; set; } = "";

        public string Summary { get; set; } = "";

        public string SectionCode { get; set; } = "";
    }
}
