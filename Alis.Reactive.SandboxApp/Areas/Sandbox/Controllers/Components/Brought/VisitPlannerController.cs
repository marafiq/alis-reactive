using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/VisitPlanner")]
    public partial class VisitPlannerController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/VisitPlanner/";

        // Seed data: each request builds its own response, so no browser world shares an object.
        private static readonly ResidentSeed[] Residents =
        [
            new ResidentSeed("margaret", "Margaret Hale", "Garden wing", 3,
            [
                new SlotSeed("mon-10", "Mon 10:00 AM", "Monday"),
                new SlotSeed("tue-15", "Tue 3:00 PM", "Tuesday"),
                new SlotSeed("wed-14", "Wed 2:00 PM", "Wednesday"),
                new SlotSeed("thu-13", "Thu 1:00 PM", "Thursday", Quiet: true),
                new SlotSeed("sat-11", "Sat 11:00 AM", "Saturday"),
                new SlotSeed("sun-11", "Sun 11:00 AM", "Sunday"),
            ]),
            new ResidentSeed("harold", "Harold Benson", "Maple wing", 2,
            [
                new SlotSeed("mon-16", "Mon 4:00 PM", "Monday"),
                new SlotSeed("fri-10", "Fri 10:00 AM", "Friday"),
                new SlotSeed("sat-14", "Sat 2:00 PM", "Saturday"),
            ]),
            new ResidentSeed("ruth", "Ruth Hale", "Garden wing", 2,
            [
                new SlotSeed("tue-10", "Tue 10:00 AM", "Tuesday"),
                new SlotSeed("thu-15", "Thu 3:00 PM", "Thursday"),
            ]),
        ];

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new FamilyVisitPlanModel());

        [HttpGet("Week/{residentId}")]
        public IActionResult Week(string residentId)
        {
            var resident = Find(residentId);
            return resident == null ? NotFound() : Ok(resident.Week());
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] FamilyVisitPlanModel? plan)
        {
            if (plan == null) return BadRequest();

            var result = new FamilyVisitPlanValidator().Validate(plan);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var resident = Find(plan.ResidentId);
            if (resident == null) return BadRequest();

            var times = plan.VisitSlots.Select(id => Array.Find(resident.Slots, slot => slot.Id == id)?.Label ?? id);
            return Ok(new VisitPlanSaved
            {
                Confirmation = $"Visits saved for {resident.Name}: {string.Join(", ", times)}.",
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

        private static ResidentSeed? Find(string? residentId) =>
            Array.Find(Residents, resident => resident.Id == residentId);

        private sealed record SlotSeed(string Id, string Label, string Day, bool Quiet = false);

        private sealed record ResidentSeed(string Id, string Name, string Wing, int WeeklyVisits, SlotSeed[] Slots)
        {
            internal ResidentWeekResponse Week() => new ResidentWeekResponse
            {
                ResidentName = Name,
                Wing = Wing,
                WeeklyVisits = WeeklyVisits,
                Slots = Slots.Select(slot => new VisitSlot { Id = slot.Id, Label = slot.Label, Day = slot.Day, Quiet = slot.Quiet }).ToList(),
            };
        }
    }
}
