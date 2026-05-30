namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the FusionCarousel sandbox demo.
    /// </summary>
    public sealed class FusionCarouselModel
    {
    }

    public sealed class FusionCarouselAuditRequest
    {
        public int CurrentIndex { get; set; }
        public int PreviousIndex { get; set; }
        public string SlideDirection { get; set; } = "";
    }

    public sealed class FusionCarouselAuditResponse
    {
        public string Message { get; set; } = "";
        public string Section { get; set; } = "";
        public string Direction { get; set; } = "";
    }
}
