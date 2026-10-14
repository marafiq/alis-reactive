using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the front desk holds a family's visit times in the community's desk system. The
    // coordinator opens the desk page from a link that names the resident and the family. Every time
    // booked in the planner is held at the desk, every time unbooked is released, and the desk answers
    // in its own words: held, held until the family confirms, taken by another family, or no answer.
    public class FrontDeskHoldsModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();

        public decimal Visitors { get; set; } = 2;
    }

    /// <summary>
    /// The times a wing's desk already holds for the family named in the request's X-Family header; the
    /// wing is named in its X-Wing header.
    /// </summary>
    public sealed class FamilyHoldsResponse
    {
        public string Family { get; set; } = "";

        /// <summary>The desk that holds the times, for example "Garden wing desk".</summary>
        public string Desk { get; set; } = "";

        public List<string> Held { get; set; } = new List<string>();
    }

    /// <summary>
    /// The desk page's link: <c>?resident=margaret&amp;family=hale</c>, which may also name how many visitors
    /// are coming (<c>&amp;visitors=3</c>) and how many the family's visitor pass allows (<c>&amp;maxVisitors=4</c>).
    /// </summary>
    public sealed class FrontDeskQuery
    {
        public string Resident { get; set; } = "";

        public string? Family { get; set; }

        public decimal Visitors { get; set; }

        public decimal MaxVisitors { get; set; }
    }

    /// <summary>A request about one resident's week at the desk: <c>{residentId}</c> in the path.</summary>
    public sealed class FrontDeskResident
    {
        public string ResidentId { get; set; } = "";
    }

    /// <summary>Releasing every hold of a resident's week: <c>{residentId}</c> in the path, the family in the body.</summary>
    public sealed class ReleaseEveryHoldRequest
    {
        public string ResidentId { get; set; } = "";

        public string? Family { get; set; }
    }

    /// <summary>The body of a release of every hold, which the desk's action binds with [FromBody].</summary>
    public class FamilyRelease
    {
        public string? Family { get; set; }
    }

    /// <summary>A request about one visit time at the desk: <c>{slotId}</c> in the path.</summary>
    public class FrontDeskSlot
    {
        public string? SlotId { get; set; }
    }

    /// <summary>The body of a hold or a badge, which the desk's actions bind with [FromBody].</summary>
    public class HoldRequest
    {
        public int VisitsThisWeek { get; set; }

        public string? Family { get; set; }

        public string? Source { get; set; }
    }

    /// <summary>A hold or a badge as the page sends it: the time in the path, the rest in the body.</summary>
    public sealed class HoldVisitRequest : HoldRequest
    {
        public string? SlotId { get; set; }
    }

    public class VisitorsChange
    {
        public decimal Visitors { get; set; }
    }

    /// <summary>A change of visitors as the page sends it: the time in the path, the count in the body.</summary>
    public sealed class ChangeVisitorsRequest : VisitorsChange
    {
        public string SlotId { get; set; } = "";
    }

    /// <summary>The desk's answer to a hold: "held" or "tentative", in its own words.</summary>
    public sealed class DeskHoldResponse
    {
        public string Status { get; set; } = "";

        public string Message { get; set; } = "";
    }

    /// <summary>The desk refused a hold: its reason, and the times it holds for the family instead.</summary>
    public sealed class DeskConflict
    {
        public string Message { get; set; } = "";

        public List<string> Held { get; set; } = new List<string>();
    }

    public sealed class DeskMessage
    {
        public string Message { get; set; } = "";
    }

    /// <summary>The card the greeter reads when a family arrives for a held visit.</summary>
    public sealed class ArrivalCard
    {
        public string Time { get; set; } = "";

        public string Room { get; set; } = "";

        public string Greeter { get; set; } = "";
    }
}
