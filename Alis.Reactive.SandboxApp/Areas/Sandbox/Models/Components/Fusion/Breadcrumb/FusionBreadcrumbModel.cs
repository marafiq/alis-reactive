namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the FusionBreadcrumb sandbox demo.
    /// </summary>
    public sealed class FusionBreadcrumbModel
    {
    }

    public sealed class FusionBreadcrumbRouteRequest
    {
        public string Text { get; set; } = "";
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public bool Disabled { get; set; }
    }

    public sealed class FusionBreadcrumbRouteResponse
    {
        public string Message { get; set; } = "";
        public string RouteCategory { get; set; } = "";
        public string Trail { get; set; } = "";
    }
}
