namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Guided Care-Plan Review journey: a nurse walks a resident through the
    /// care-plan sections one slide at a time. The model carries no input fields;
    /// the carousel is a navigation/display component driven entirely by the plan.
    /// </summary>
    public sealed class FusionCarouselModel
    {
    }

    /// <summary>
    /// One reviewed section, gathered from the carousel slide-change payload and
    /// posted to the resident's chart as the review progresses.
    /// </summary>
    public sealed class CarePlanReviewEntry
    {
        /// <summary>Section index the resident landed on (carousel currentIndex).</summary>
        public int SectionIndex { get; set; }

        /// <summary>Section index the resident came from (carousel previousIndex).</summary>
        public int CameFromIndex { get; set; }

        /// <summary>Direction of the move: "Next" or "Previous" (carousel slideDirection).</summary>
        public string Direction { get; set; } = "";

        /// <summary>Whether the resident reached this section by swipe (carousel isSwiped).</summary>
        public bool BySwipe { get; set; }
    }

    /// <summary>
    /// The chart confirmation shown after a reviewed section is recorded.
    /// </summary>
    public sealed class CarePlanReviewResponse
    {
        /// <summary>Name of the section that was recorded, e.g. "Discharge Steps".</summary>
        public string Section { get; set; } = "";

        /// <summary>Name of the section the resident came from, e.g. "Therapy Goals".</summary>
        public string CameFrom { get; set; } = "";

        /// <summary>How the resident moved, in chart words: "moved forward to" or "went back to".</summary>
        public string Movement { get; set; } = "";

        /// <summary>How the resident navigated, in chart words: "using the buttons" or "by swiping".</summary>
        public string NavigatedBy { get; set; } = "";

        /// <summary>The full chart line the nurse sees confirming the recorded review.</summary>
        public string ChartLine { get; set; } = "";
    }
}
