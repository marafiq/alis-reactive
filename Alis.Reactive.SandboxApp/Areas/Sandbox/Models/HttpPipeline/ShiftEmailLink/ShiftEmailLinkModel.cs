namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A resident's file, opened from the link in the shift email. The link names everything the page
    /// asks for; the page itself has no inputs.
    /// </summary>
    public class ShiftEmailLinkPageModel
    {
    }

    /// <summary>
    /// The shift email's link:
    /// <c>?community=maple-grove&amp;resident=1042&amp;shift=Night&amp;tab=medications&amp;page=2&amp;wing=Maple&amp;wing=Cedar</c>.
    /// It names the community and the resident, the shift the email was sent for, the section to open,
    /// the page of shift notes, and every wing the nurse covers.
    /// </summary>
    public sealed class ShiftEmailLinkQuery
    {
        public string Community { get; set; } = "";

        public int Resident { get; set; }

        public string Shift { get; set; } = "";

        public string Tab { get; set; } = "";

        public int Page { get; set; }

        public string[] Wing { get; set; } = Array.Empty<string>();
    }

    /// <summary>The chart request <c>Residents/{residentId}/Chart?shift=</c> binds: the resident in the path, the shift in the query.</summary>
    public sealed class ShiftEmailChartRequest
    {
        public int ResidentId { get; set; }

        public string Shift { get; set; } = "";
    }

    /// <summary>
    /// A resident's chart, as the community's API answers it: where the resident lives and the dose due
    /// on the shift asked for. <paramref name="DueMedication"/> names that dose's medication.
    /// </summary>
    public sealed record ShiftEmailChart(string Name, string Room, string Community, string DueDose, string DueMedication);

    /// <summary>Why the community's API would not answer.</summary>
    public sealed record ShiftEmailRefusal(string Message);

    /// <summary>The request <c>Residents/{residentId}/Doses/{medication}</c> binds: both in the path.</summary>
    public sealed class ShiftEmailDoseRequest
    {
        public int ResidentId { get; set; }

        public string Medication { get; set; } = "";
    }

    /// <summary>When a medication was last given, and by whom.</summary>
    public sealed record ShiftEmailLastDose(string LastGiven);

    /// <summary>The request <c>Residents/{residentId}/Notes?shift=&amp;page=</c> binds: the resident in the path, the rest in the query.</summary>
    public sealed class ShiftEmailNotesRequest
    {
        public int ResidentId { get; set; }

        public string Shift { get; set; } = "";

        public int Page { get; set; }
    }

    /// <summary>The shift note on one page of a resident's shift notes.</summary>
    public sealed record ShiftEmailNotes(string Entry);

    /// <summary>The partner check <c>Residents/{residentId}/Partners</c> binds: the resident in the path; the partner rides in a header.</summary>
    public sealed class ShiftEmailPartnerRequest
    {
        public int ResidentId { get; set; }
    }

    /// <summary>A partner's answer about the resident, in the partner's own words.</summary>
    public sealed record ShiftEmailPartnerAnswer(string Answer);
}
