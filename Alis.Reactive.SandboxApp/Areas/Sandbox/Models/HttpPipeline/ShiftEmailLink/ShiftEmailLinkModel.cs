namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ShiftEmailLinkPageModel
    {
    }

    public sealed class ShiftEmailLinkQuery
    {
        public string Community { get; set; } = "";

        public int Resident { get; set; }

        public string Shift { get; set; } = "";

        public string Tab { get; set; } = "";

        public int Page { get; set; }

        public string[] Wing { get; set; } = Array.Empty<string>();
    }

    public sealed class ShiftEmailChartRequest
    {
        public int ResidentId { get; set; }

        public string Shift { get; set; } = "";
    }

    public sealed record ShiftEmailChart(string Name, string Room, string Community, string DueDose, string DueMedication);

    public sealed record ShiftEmailRefusal(string Message);

    public sealed class ShiftEmailDoseRequest
    {
        public int ResidentId { get; set; }

        public string Medication { get; set; } = "";
    }

    public sealed record ShiftEmailLastDose(string LastGiven);

    public sealed class ShiftEmailNotesRequest
    {
        public int ResidentId { get; set; }

        public string Shift { get; set; } = "";

        public int Page { get; set; }
    }

    public sealed record ShiftEmailNotes(string Entry);

    public sealed class ShiftEmailPartnerRequest
    {
        public int ResidentId { get; set; }
    }

    public sealed record ShiftEmailPartnerAnswer(string Answer);
}
