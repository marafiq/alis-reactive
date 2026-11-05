namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CarePlanReviewsViewModel
    {
        public List<CarePlanReviewDue> DueThisWeek { get; set; } = new();
    }

    public sealed record CarePlanReviewDue(string ButtonId, int ResidentId, string Label);

    public class CarePlanReview
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string CareLevel { get; set; } = "";

        public string NextReview { get; set; } = "";
    }

    public class CarePlanReviewBooking
    {
        public CarePlanReview Review { get; set; } = new();
    }

    public class CarePlanReviewAnswer
    {
        public CarePlanReview Review { get; set; } = new();
        public string Summary { get; set; } = "";
    }
}
