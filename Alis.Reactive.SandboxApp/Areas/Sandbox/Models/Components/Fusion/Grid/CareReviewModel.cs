namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class WingReviewViewModel
    {
        public string Wing { get; set; } = "";
        public string Director { get; set; } = "";

        public int PageSize { get; set; }
    }

    public class WingReviewResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string ReviewDue { get; set; } = "";

        public string Review { get; set; } = "";
    }

    public class WingReviewStart
    {
        public int ResidentId { get; set; }
    }

    public class WingReviewStarted
    {
        public int ResidentId { get; set; }
        public string Review { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    public class WingReviewSuiteVisit
    {
        public string Suite { get; set; } = "";
    }

    public class WingReviewSuiteReady
    {
        public string Summary { get; set; } = "";
    }

    public class WingReviewConference
    {
        public List<WingReviewResident> Residents { get; set; } = new();
    }

    public class WingReviewAgenda
    {
        public string Summary { get; set; } = "";
    }

    public class WingReviewRefusal
    {
        public string Summary { get; set; } = "";
    }

    public class WingReviewWaitingResident
    {
        public string ResidentName { get; set; } = "";

        public int? Bed { get; set; }
    }

    public class WingReviewBedHold
    {
        public int? Bed { get; set; }
    }

    public static class WingReviewEvents
    {
        public static readonly AppEvent<WingReviewStart> StartReview = AppEvent.Declare<WingReviewStart>();

        public static readonly AppEvent<WingReviewSuiteVisit> ReadySuite = AppEvent.Declare<WingReviewSuiteVisit>();

        public static readonly AppEvent<WingReviewBedHold> KeepBed = AppEvent.Declare<WingReviewBedHold>();
    }
}
