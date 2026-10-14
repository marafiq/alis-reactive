using System.Collections.Generic;
using Alis.Reactive.FluentValidator;
using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    // Journey: the life enrichment coordinator plans a family's visits with a resident for the week.
    // The week's times come from the resident's wing; quiet hours cannot be booked; the week allows a
    // set number of visits; the server knows the therapy schedule. The visit planner is a widget the
    // sandbox wrote itself and brings like any app would (BroughtComponents/VisitPlanner).
    public class FamilyVisitPlanModel
    {
        public string? ResidentId { get; set; }

        public List<string> VisitSlots { get; set; } = new List<string>();

        public decimal Visitors { get; set; } = 2;

        public string? Note { get; set; }

        /// <summary>A time the family asked for, checked against the planner.</summary>
        public string? RequestedTime { get; set; }

        public bool LockWeek { get; set; }

        /// <summary>When the week is full, a new time replaces the earliest one.</summary>
        public bool SwapWhenFull { get; set; }
    }

    // A companion moving in with the resident plans their own visits in a panel loaded on request.
    public class CompanionVisitPlanModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();
    }

    /// <summary>A request for one resident's week of visit times from the Week endpoint: <c>{residentId}</c> in the path.</summary>
    public sealed class ResidentWeekRequest
    {
        public string? ResidentId { get; set; }
    }

    public sealed class ResidentWeekResponse
    {
        public string ResidentName { get; set; } = "";

        public string Wing { get; set; } = "";

        public int WeeklyVisits { get; set; }

        public List<VisitSlot> Slots { get; set; } = new List<VisitSlot>();
    }

    public sealed class VisitPlanSaved
    {
        public string Confirmation { get; set; } = "";
    }

    public sealed class CompanionHoldResponse
    {
        public string Held { get; set; } = "";
    }

    public class FamilyVisitPlanValidator : ReactiveValidator<FamilyVisitPlanModel>
    {
        public FamilyVisitPlanValidator()
        {
            RuleFor(x => x.ResidentId).NotEmpty().WithMessage("Choose the resident the family is visiting.");
            RuleFor(x => x.VisitSlots).NotEmpty().WithMessage("Plan at least one visit.");

            // The therapy schedule lives on the server: a Must the browser cannot run.
            RuleFor(x => x.VisitSlots)
                .Must((plan, slots) => !(plan.ResidentId == "margaret" && slots.Contains("wed-14")))
                .WithMessage("Wed 2:00 PM is Margaret's physical therapy; choose another time.");
        }
    }
}
