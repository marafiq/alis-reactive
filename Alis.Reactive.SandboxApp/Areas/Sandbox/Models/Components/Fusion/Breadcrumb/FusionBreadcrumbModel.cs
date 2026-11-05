namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionBreadcrumbModel
    {
        public string CommunityName { get; set; } = "Sunrise Court";

        public string ResidentName { get; set; } = "Eleanor Hughes";
    }

    public sealed class OpenCareSectionRequest
    {
        public string Text { get; set; } = "";

        public string Id { get; set; } = "";

        public string Url { get; set; } = "";

        public bool Disabled { get; set; }
    }

    public sealed class OpenCareSectionResponse
    {
        public string Heading { get; set; } = "";

        public string Summary { get; set; } = "";

        public string SectionCode { get; set; } = "";
    }
}
