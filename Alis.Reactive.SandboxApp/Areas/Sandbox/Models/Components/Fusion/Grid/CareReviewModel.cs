namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The director of nursing's care reviews for one wing. The grid holds the residents, so the page has no
    /// fields of its own.
    /// </summary>
    public class WingReviewViewModel
    {
        public string Wing { get; set; } = "";
        public string Director { get; set; } = "";

        /// <summary>How many residents the grid shows on a page.</summary>
        public int PageSize { get; set; }
    }

    /// <summary>A resident on the wing's review list, one row of the grid.</summary>
    public class WingReviewResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Suite { get; set; } = "";
        public string RiskLevel { get; set; } = "";
        public string ReviewDue { get; set; } = "";

        /// <summary>Where the resident's care review stands: <c>Not started</c> or <c>Under way</c>.</summary>
        public string Review { get; set; } = "";
    }

    /// <summary>The resident whose review the director starts from their row.</summary>
    public class WingReviewStart
    {
        public int ResidentId { get; set; }
    }

    /// <summary>A review the server started: the resident's key, where the review stands now, and what the page says.</summary>
    public class WingReviewStarted
    {
        public int ResidentId { get; set; }
        public string Review { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    /// <summary>The residents the director picked for the care conference.</summary>
    public class WingReviewConference
    {
        public List<WingReviewResident> Residents { get; set; } = new();
    }

    /// <summary>What the server says about the care conference agenda.</summary>
    public class WingReviewAgenda
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>Why a review was not started.</summary>
    public class WingReviewRefusal
    {
        public string Summary { get; set; } = "";
    }

    /// <summary>The document events the wing's review list raises and hears.</summary>
    public static class WingReviewEvents
    {
        /// <summary>Raised by a resident's Start review button with that resident's key.</summary>
        public static readonly AppEvent<WingReviewStart> StartReview = AppEvent.Declare<WingReviewStart>();
    }
}
