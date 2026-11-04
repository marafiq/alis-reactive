namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionCarouselModel
    {
    }

    public sealed class CarePlanReviewEntry
    {
        public int SectionIndex { get; set; }

        public int CameFromIndex { get; set; }

        public string Direction { get; set; } = "";

        public bool BySwipe { get; set; }
    }

    public sealed class CarePlanReviewResponse
    {
        public string Section { get; set; } = "";

        public string CameFrom { get; set; } = "";

        public string Movement { get; set; } = "";

        public string NavigatedBy { get; set; } = "";

        public string ChartLine { get; set; } = "";
    }
}
