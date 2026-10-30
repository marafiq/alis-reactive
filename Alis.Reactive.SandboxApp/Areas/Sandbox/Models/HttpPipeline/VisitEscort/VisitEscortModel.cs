using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Visit escorts: a memory-care resident's family visits need an escort from the wing's desk. The
    /// coordinator books the week's times in the visit planner, a widget the sandbox wrote itself; each
    /// time booked asks the desk the link names for an escort, each time released releases the escort.
    /// </summary>
    public class VisitEscortModel
    {
        /// <summary>The visit times booked this week; the planner's value.</summary>
        public List<string> VisitSlots { get; set; } = new();

        /// <summary>How many visitors come to the earliest visit.</summary>
        public string? Visitors { get; set; } = "2";

        /// <summary>A note for the desk about the earliest visit.</summary>
        public string? Note { get; set; }

        /// <summary>Who comes along to the earliest visit.</summary>
        public string? Companion { get; set; }
    }

    /// <summary>
    /// The page's link: <c>?resident=helen&amp;wing=Maple&amp;desk=east</c>. It names the resident, the wing and
    /// the desk that books the escorts; a link may leave the desk out.
    /// </summary>
    public sealed class VisitEscortLink
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        public string? Desk { get; set; }
    }

    /// <summary>The request <c>Week/{resident}</c> binds: the resident in the path.</summary>
    public sealed class EscortWeekRequest
    {
        public string Resident { get; set; } = "";
    }

    /// <summary>A resident's week of visit times, as the wing's desk offers it.</summary>
    public sealed class EscortWeek
    {
        public string Resident { get; set; } = "";

        public string Wing { get; set; } = "";

        public int Capacity { get; set; }

        public List<VisitSlot> Slots { get; set; } = new();
    }

    /// <summary>
    /// The request <c>GET Escorts/{slot}?wing=&amp;visit=</c> binds: the time in the path, the wing and which
    /// visit of the week in the query; the desk rides in the X-Desk header.
    /// </summary>
    public sealed class EscortRequest
    {
        public string Slot { get; set; } = "";

        public string Wing { get; set; } = "";

        public int Visit { get; set; }
    }

    /// <summary>The request <c>DELETE Escorts/{slot}</c> binds: the time in the path, the reason in the body.</summary>
    public sealed class EscortRelease
    {
        public string Slot { get; set; } = "";

        public string? Reason { get; set; }
    }

    /// <summary>The body <c>POST Share</c> binds: the summary the planner prepared, and the visits left.</summary>
    public sealed class WeekShare
    {
        public string? Summary { get; set; }

        public int VisitsLeft { get; set; }
    }

    /// <summary>The request <c>PUT Party/{slot}</c> binds: the time in the path, the party in the body.</summary>
    public sealed class PartyUpdate
    {
        public string Slot { get; set; } = "";

        public VisitParty Party { get; set; } = new();

        public List<VisitCompanion> Companions { get; set; } = new();
    }

    /// <summary>Who comes to a visit.</summary>
    public class VisitParty
    {
        public string? Visitors { get; set; }

        public string? Note { get; set; }
    }

    /// <summary>Someone who comes along.</summary>
    public class VisitCompanion
    {
        public string? Name { get; set; }
    }

    /// <summary>The query <c>GET Seating</c> binds: the party, as the nested and indexed keys MVC reads.</summary>
    public sealed class PartyQuery
    {
        public VisitParty Party { get; set; } = new();

        public List<VisitCompanion> Companions { get; set; } = new();
    }

    /// <summary>What the desk says, in its own words.</summary>
    public sealed record EscortMessage(string Message);
}
