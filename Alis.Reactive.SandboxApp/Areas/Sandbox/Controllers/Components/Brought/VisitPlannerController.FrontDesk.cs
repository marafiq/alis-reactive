using System;
using System.Collections.Generic;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    // The front desk's system, answering by the time asked for: Monday 10:00 is held, Tuesday 3:00 is
    // held until the family confirms, Saturday 11:00 was taken by another family, and on Sunday the
    // desk does not answer. The Hale family already holds Wednesday 2:00.
    public partial class VisitPlannerController
    {
        [HttpGet("FrontDesk")]
        public IActionResult FrontDesk() => View(ViewBase + "FrontDesk.cshtml", new FrontDeskHoldsModel());

        [HttpGet("FrontDesk/Week/{residentId}")]
        public IActionResult FrontDeskWeek(string residentId)
        {
            var resident = Find(residentId);
            return resident == null
                ? NotFound(new DeskMessage { Message = "No resident matches this link." })
                : Ok(resident.Week());
        }

        [HttpGet("FrontDesk/Holds/{residentId}")]
        public IActionResult FrontDeskHolds(string residentId)
        {
            var family = Request.Headers["X-Family"].ToString();
            if (string.IsNullOrEmpty(family)) return BadRequest(new DeskMessage { Message = "Name the family." });

            return Ok(new FamilyHoldsResponse { Family = $"{Capitalized(family)} family", Held = new List<string> { "wed-14" } });
        }

        [HttpPost("FrontDesk/Hold/{slotId}")]
        public IActionResult HoldVisit(string slotId, [FromBody] HoldRequest? hold)
        {
            var wing = Request.Headers["X-Wing"].ToString();
            var family = Capitalized(hold?.Family ?? "");
            var label = LabelOf(slotId);
            switch (slotId)
            {
                case "tue-15":
                    return Ok(new DeskHoldResponse { Status = "tentative", Message = $"{label} is held until the {family} family confirms." });
                case "sat-11":
                    return Conflict(new DeskConflict
                    {
                        Message = $"{label} was just taken by the Okafor family.",
                        Held = new List<string> { "wed-14" },
                    });
                case "sun-11":
                    return StatusCode(503);
                default:
                    return Ok(new DeskHoldResponse
                    {
                        Status = "held",
                        Message = $"Held {label} for the {family} family at the {wing} desk; visit {hold?.VisitsThisWeek} this week.",
                    });
            }
        }

        [HttpPut("FrontDesk/Hold/{slotId}")]
        public IActionResult ChangeVisitors(string slotId, [FromBody] VisitorsChange? change) =>
            Ok(new DeskMessage { Message = $"{LabelOf(slotId)} now expects {change?.Visitors:0} visitors." });

        [HttpDelete("FrontDesk/Hold/{slotId}")]
        public IActionResult ReleaseHold(string slotId) =>
            Ok(new DeskMessage { Message = $"Released {LabelOf(slotId)}." });

        [HttpDelete("FrontDesk/Holds/{residentId}")]
        public IActionResult ReleaseEveryHold(string residentId) =>
            Ok(new DeskMessage { Message = $"Released every hold for {Find(residentId)?.Name ?? residentId}." });

        private static string LabelOf(string slotId)
        {
            foreach (var resident in Residents)
            {
                var slot = Array.Find(resident.Slots, candidate => candidate.Id == slotId);
                if (slot != null) return slot.Label;
            }

            return slotId;
        }

        private static string Capitalized(string name) =>
            name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);
    }
}
