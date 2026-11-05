using System.Collections.Generic;
using Alis.Reactive.SandboxApp.BroughtComponents;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought
{
    public class FamilyVisitPlanModel
    {
        public string? ResidentId { get; set; }

        public List<string> VisitSlots { get; set; } = new List<string>();

        public decimal Visitors { get; set; } = 2;

        public string? Note { get; set; }

        public string? RequestedTime { get; set; }

        public bool LockWeek { get; set; }

        public bool SwapWhenFull { get; set; }
    }

    public class CompanionVisitPlanModel
    {
        public List<string> VisitSlots { get; set; } = new List<string>();
    }

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

            RuleFor(x => x.VisitSlots)
                .Must((plan, slots) => !(plan.ResidentId == "margaret" && slots.Contains("wed-14")))
                .WithMessage("Wed 2:00 PM is Margaret's physical therapy; choose another time.");
        }
    }
}
