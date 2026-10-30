using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the admissions coordinator sends the wing's night desk a move-in notice for an applicant.
    // Every fact the notice carries comes from where it lives: the intake sheet (a widget the sandbox
    // wrote itself, BroughtComponents/IntakeSheet), its contact object, what its functions return, the
    // link that opened the page, the notice the page raises, the page's plugins, and what the page wrote
    // itself; and each fact goes wherever the notice takes it: the page, a raised event, a plugin, the
    // desk's address, its headers and its body.
    public class MoveInNoticeModel
    {
        /// <summary>The id of the applicant's intake sheet on the page.</summary>
        public const string SheetId = "intake";

        /// <summary>Margaret Hale, as the waiting list holds her.</summary>
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

        /// <summary>Her number on the waiting list when the page opens.</summary>
        public int WaitlistPosition { get; set; } = 2;
    }

    /// <summary>The link that opens the notice: <c>?resident=margaret&amp;wing=garden&amp;room=Garden%20214&amp;visitors=3</c>.</summary>
    public sealed class MoveInLink
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        /// <summary>The room offered, when the link names one.</summary>
        public string? Room { get; set; }

        /// <summary>How many come on move-in day, when the link says.</summary>
        public int? Visitors { get; set; }
    }

    /// <summary>What the waiting list is asked: the resident in the address, the wing in the query.</summary>
    public sealed class WaitingListQuery
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";
    }

    /// <summary>The waiting list's record of an applicant.</summary>
    public sealed class WaitingListEntry
    {
        public string Wing { get; set; } = "";

        public int? Position { get; set; }

        public Applicant Applicant { get; set; } = new Applicant();
    }

    /// <summary>The notice the page raises before it goes to the desk.</summary>
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

        /// <summary>The applicant's record as the notice carries it, when it carries one.</summary>
        public Applicant? Record { get; set; }

        public CareLevel CareLevel { get; set; }

        public DateTime MoveInDate { get; set; }

        public bool Veteran { get; set; }

        public int Age { get; set; }
    }

    /// <summary>The family contact as the notice names them.</summary>
    public sealed class NoticeContact
    {
        public string Relation { get; set; } = "";
    }

    /// <summary>An edit of the sheet, raised with the applicant as the sheet then holds her.</summary>
    public sealed class SheetEdit
    {
        public string Field { get; set; } = "";

        public Applicant Applicant { get; set; } = new Applicant();
    }

    /// <summary>The events the move-in notice raises and hears.</summary>
    public static class MoveInEvents
    {
        /// <summary>The coordinator asked for the notice to go to the desk.</summary>
        public static readonly AppEvent<MoveInNotice> NoticeRequested = AppEvent.Declare<MoveInNotice>();

        /// <summary>The coordinator edited the sheet.</summary>
        public static readonly AppEvent<SheetEdit> SheetEdited = AppEvent.Declare<SheetEdit>();
    }

    /// <summary>
    /// What the desk is sent. The members named like the address's placeholders fill the address; the
    /// others go in the body.
    /// </summary>
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

    /// <summary>The desk's receipt: what reached it by the address, the headers and the body.</summary>
    public sealed class NoticeReceipt
    {
        public string Route { get; set; } = "";

        public string Headers { get; set; } = "";

        public string Body { get; set; } = "";
    }
}
