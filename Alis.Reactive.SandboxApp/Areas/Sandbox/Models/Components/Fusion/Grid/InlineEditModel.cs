namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The care-plan review page: the reviews that fall due this week, each opened from its own button.</summary>
    public class CarePlanReviewsViewModel
    {
        /// <summary>The reviews due by the end of this week, soonest first.</summary>
        public List<CarePlanReviewDue> DueThisWeek { get; set; } = new();
    }

    /// <summary>A review due this week: the button that opens it, whose review it is, and the button's words.</summary>
    public sealed record CarePlanReviewDue(string ButtonId, int ResidentId, string Label);

    /// <summary>A resident's next care-plan review, one row of the review list.</summary>
    public class CarePlanReview
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";

        /// <summary>The day of the next review, as the date editor writes it: <c>2026-10-16</c>.</summary>
        public string NextReview { get; set; } = "";
    }

    /// <summary>A review the nurse moved, sent to the care team's calendar.</summary>
    public class CarePlanReviewBooking
    {
        public CarePlanReview Review { get; set; } = new();
    }

    /// <summary>The calendar's answer: the review as it now stands, and what the nurse is told.</summary>
    public class CarePlanReviewAnswer
    {
        public CarePlanReview Review { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
