using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/VisitPlanner")]
    public class VisitPlannerController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/VisitPlanner/";

        private static readonly Dictionary<string, ResidentWeekResponse> Weeks = new Dictionary<string, ResidentWeekResponse>
        {
            ["margaret"] = Week("Margaret Hale", "Garden wing", 3,
                Slot("mon-10", "Mon 10:00 AM", "Monday"),
                Slot("tue-15", "Tue 3:00 PM", "Tuesday"),
                Slot("wed-14", "Wed 2:00 PM", "Wednesday"),
                Slot("thu-13", "Thu 1:00 PM", "Thursday", quiet: true),
                Slot("sat-11", "Sat 11:00 AM", "Saturday"),
                Slot("sun-11", "Sun 11:00 AM", "Sunday")),
            ["harold"] = Week("Harold Benson", "Maple wing", 2,
                Slot("mon-16", "Mon 4:00 PM", "Monday"),
                Slot("fri-10", "Fri 10:00 AM", "Friday"),
                Slot("sun-11", "Sun 11:00 AM", "Sunday")),
            ["ruth"] = Week("Ruth Hale", "Garden wing", 2,
                Slot("tue-10", "Tue 10:00 AM", "Tuesday"),
                Slot("thu-15", "Thu 3:00 PM", "Thursday")),
        };

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new FamilyVisitPlanModel());

        [HttpGet("Week/{residentId}")]
        public IActionResult Week(string residentId) =>
            Weeks.TryGetValue(residentId, out var week) ? Ok(week) : NotFound();

        [HttpPost("Save")]
        public IActionResult Save([FromBody] FamilyVisitPlanModel? plan)
        {
            if (plan == null) return BadRequest();

            var result = new FamilyVisitPlanValidator().Validate(plan);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            if (!Weeks.TryGetValue(plan.ResidentId!, out var week)) return BadRequest();

            var times = plan.VisitSlots.Select(id => week.Slots.FirstOrDefault(slot => slot.Id == id)?.Label ?? id);
            return Ok(new VisitPlanSaved
            {
                Confirmation = $"Visits saved for {week.ResidentName}: {string.Join(", ", times)}.",
            });
        }

        [HttpGet("Companion")]
        public IActionResult Companion() =>
            PartialView(ViewBase + "_CompanionVisits.cshtml", new CompanionVisitPlanModel());

        [HttpGet("NoCompanion")]
        public IActionResult NoCompanion() => PartialView(ViewBase + "_NoCompanion.cshtml");

        // The front desk holds a companion's time as soon as it is booked.
        [HttpPost("CompanionHold")]
        public IActionResult CompanionHold([FromBody] CompanionVisitPlanModel? plan) =>
            Ok(new CompanionHoldResponse { Held = string.Join(", ", plan?.VisitSlots ?? new List<string>()) });

        private static ResidentWeekResponse Week(string name, string wing, int visits, params VisitSlot[] slots) =>
            new ResidentWeekResponse { ResidentName = name, Wing = wing, WeeklyVisits = visits, Slots = slots.ToList() };

        private static VisitSlot Slot(string id, string label, string day, bool quiet = false) =>
            new VisitSlot { Id = id, Label = label, Day = day, Quiet = quiet };
    }
}
