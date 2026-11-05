using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class MoveInNoticeModel
    {
        public const string SheetId = "intake";

        public Applicant Applicant { get; set; } = new Applicant
        {
            Name = "Margaret Hale",
            Age = 62,
            MoveInDate = new DateTime(2026, 7, 1),
            Veteran = true,
            CareLevel = CareLevel.Assisted,
            MonthlyRate = 5000.00m,
            PetCount = 1,
            Contact = new ApplicantContact { Phone = "555-0101", Relation = "Daughter" },
            Allergies = ["Penicillin"],
        };

        public int WaitlistPosition { get; set; } = 2;
    }

    public sealed class MoveInLink
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        public string? Room { get; set; }

        public int? Visitors { get; set; }
    }

    public sealed class WaitingListQuery
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";
    }

    public sealed class WaitingListEntry
    {
        public string Wing { get; set; } = "";

        public int? Position { get; set; }

        public Applicant Applicant { get; set; } = new Applicant();
    }

    public sealed class MoveInNotice
    {
        public string Applicant { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Summary { get; set; } = "";

        public int? Position { get; set; }

        public string Wing { get; set; } = "";

        public string Source { get; set; } = "";

        public IReadOnlyList<string> Allergies { get; set; } = new List<string>();

        public List<string> Services { get; set; } = new List<string>();

        public NoticeContact Contact { get; set; } = new NoticeContact();

        public Applicant? Record { get; set; }

        public CareLevel CareLevel { get; set; }

        public DateTime MoveInDate { get; set; }

        public bool Veteran { get; set; }

        public int Age { get; set; }
    }

    public sealed class NoticeContact
    {
        public string Relation { get; set; } = "";
    }

    public sealed class SheetEdit
    {
        public string Field { get; set; } = "";

        public Applicant Applicant { get; set; } = new Applicant();
    }

    public static class MoveInEvents
    {
        public static readonly AppEvent<MoveInNotice> NoticeRequested = AppEvent.Declare<MoveInNotice>();

        public static readonly AppEvent<SheetEdit> SheetEdited = AppEvent.Declare<SheetEdit>();
    }

    public sealed class NoticeRequest
    {
        public string Wing { get; set; } = "";

        public string Desk { get; set; } = "";

        public string Applicant { get; set; } = "";

        public int Age { get; set; }

        public string Phone { get; set; } = "";

        public int Days { get; set; }

        public int? Position { get; set; }

        public string Relation { get; set; } = "";

        public string Name { get; set; } = "";

        public string ContactPhone { get; set; } = "";

        public string Summary { get; set; } = "";

        public int? ListPosition { get; set; }

        public string WingName { get; set; } = "";

        public string Source { get; set; } = "";

        public string Notice { get; set; } = "";

        public string ContactRelation { get; set; } = "";

        public List<string> Allergies { get; set; } = new List<string>();

        public List<string> Services { get; set; } = new List<string>();

        public string Code { get; set; } = "";

        public CareLevel CareLevel { get; set; }

        public DateTime MoveInDate { get; set; }

        public bool Veteran { get; set; }

        public ApplicantContact Facts { get; set; } = new ApplicantContact();

        public int Years { get; set; }
    }

    public sealed class NoticeReceipt
    {
        public string Route { get; set; } = "";

        public string Headers { get; set; } = "";

        public string Body { get; set; } = "";
    }
}
