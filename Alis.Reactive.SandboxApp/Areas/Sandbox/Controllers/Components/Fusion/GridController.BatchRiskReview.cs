using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The morning falls huddle on Aspen wing: the director of nursing raises the fall risk of the residents who fell
    /// overnight and sends the changed risk levels to care planning. Every page starts from the risk levels on record;
    /// the server says what care planning will update against them and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BatchRiskReview"), ActionName("BatchRiskReview")]
        public IActionResult FallsHuddlePage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BatchRiskReview.cshtml",
                new FallsHuddleViewModel
                {
                    Falls = new List<OvernightFall>
                    {
                        new() { ResidentId = 6402, ResidentName = "Henry Liu", WhatHappened = "found on the floor beside his bed at 03:10" },
                        new() { ResidentId = 6405, ResidentName = "Ruth Carter", WhatHappened = "slid from her chair at 05:40, no injury" }
                    }
                });

        [HttpGet("FallsHuddleResidents")]
        public IActionResult FallsHuddleResidents() => Ok(FallsHuddleOnRecord());

        [HttpPost("FallsHuddleSend")]
        public IActionResult FallsHuddleSend([FromBody] FallsHuddleChanges huddle)
        {
            var onRecord = FallsHuddleOnRecord().ToDictionary(r => r.ResidentId);
            var updates = huddle.Changes.ChangedRecords
                .Where(changed => onRecord[changed.ResidentId].FallRisk != changed.FallRisk)
                .Select(changed => $"{changed.ResidentName}, {onRecord[changed.ResidentId].FallRisk} → {changed.FallRisk}")
                .ToList();

            return Ok(new FallsHuddleSent
            {
                Summary = updates.Count == 0
                    ? "No fall risk changed, so nothing went to care planning."
                    : $"Sent to care planning: {string.Join("; ", updates)}."
            });
        }

        private static List<FallsHuddleResident> FallsHuddleOnRecord() => new()
        {
            new FallsHuddleResident { ResidentId = 6401, ResidentName = "Amina Patel", Room = "201", FallRisk = "Low" },
            new FallsHuddleResident { ResidentId = 6402, ResidentName = "Henry Liu", Room = "204", FallRisk = "Moderate" },
            new FallsHuddleResident { ResidentId = 6403, ResidentName = "Grace Bennett", Room = "208", FallRisk = "Low" },
            new FallsHuddleResident { ResidentId = 6404, ResidentName = "Walter Grant", Room = "212", FallRisk = "Moderate" },
            new FallsHuddleResident { ResidentId = 6405, ResidentName = "Ruth Carter", Room = "215", FallRisk = "Low" }
        };
    }
}
