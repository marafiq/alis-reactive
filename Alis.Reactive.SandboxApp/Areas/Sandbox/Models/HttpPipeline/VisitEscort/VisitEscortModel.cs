using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class VisitEscortModel
    {
        public List<string> VisitSlots { get; set; } = new();

        public string? Visitors { get; set; } = "2";

        public string? Note { get; set; }

        public string? Companion { get; set; }
    }

    public sealed class VisitEscortLink
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        public string? Desk { get; set; }
    }

    public sealed class EscortWeekRequest
    {
        public string Resident { get; set; } = "";
    }

    public sealed class EscortWeek
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        public int Capacity { get; set; }

        public List<VisitSlot> Slots { get; set; } = new();
    }

    public sealed class EscortRequest
    {
        public string Slot { get; set; } = "";

        public string Wing { get; set; } = "";

        public int Visit { get; set; }
    }

    public sealed class EscortRelease
    {
        public string Slot { get; set; } = "";

        public string? Reason { get; set; }
    }

    public sealed class WeekShare
    {
        public string? Summary { get; set; }

        public int VisitsLeft { get; set; }
    }

    public sealed class PartyUpdate
    {
        public string Slot { get; set; } = "";

        public VisitParty Party { get; set; } = new();

        public List<VisitCompanion> Companions { get; set; } = new();
    }

    public class VisitParty
    {
        public string? Visitors { get; set; }

        public string? Note { get; set; }
    }

    public class VisitCompanion
    {
        public string? Name { get; set; }
    }

    public sealed class PartyQuery
    {
        public VisitParty Party { get; set; } = new();

        public List<VisitCompanion> Companions { get; set; } = new();
    }

    public sealed record EscortMessage(string Message);
}
