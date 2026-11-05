using System.Collections.Generic;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class FrontDeskHoldsModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();

        public decimal Visitors { get; set; } = 2;
    }

    public sealed class FamilyHoldsResponse
    {
        public string Family { get; set; } = "";

        public string Desk { get; set; } = "";

        public List<string> Held { get; set; } = new List<string>();
    }

    public sealed class FrontDeskQuery
    {
        public string Resident { get; set; } = "";

        public string? Family { get; set; }

        public decimal Visitors { get; set; }

        public decimal MaxVisitors { get; set; }
    }

    public sealed class FrontDeskResident
    {
        public string ResidentId { get; set; } = "";
    }

    public sealed class ReleaseEveryHoldRequest
    {
        public string ResidentId { get; set; } = "";

        public string? Family { get; set; }
    }

    public class FamilyRelease
    {
        public string? Family { get; set; }
    }

    public class FrontDeskSlot
    {
        public string? SlotId { get; set; }
    }

    public class HoldRequest
    {
        public int VisitsThisWeek { get; set; }

        public string? Family { get; set; }

        public string? Source { get; set; }
    }

    public sealed class HoldVisitRequest : HoldRequest
    {
        public string? SlotId { get; set; }
    }

    public class VisitorsChange
    {
        public decimal Visitors { get; set; }
    }

    public sealed class ChangeVisitorsRequest : VisitorsChange
    {
        public string SlotId { get; set; } = "";
    }

    public sealed class DeskHoldResponse
    {
        public string Status { get; set; } = "";

        public string Message { get; set; } = "";
    }

    public sealed class DeskConflict
    {
        public string Message { get; set; } = "";

        public List<string> Held { get; set; } = new List<string>();
    }

    public sealed class DeskMessage
    {
        public string Message { get; set; } = "";
    }

    public sealed class ArrivalCard
    {
        public string Time { get; set; } = "";

        public string Room { get; set; } = "";

        public string Greeter { get; set; } = "";
    }
}
