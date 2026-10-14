using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The resident roster as the resident record lists it: resident services brings today's changes from the record
    /// onto the roster (an arrival, a care level, an approved suite move) and looks at another wing. Every page starts
    /// from the record; each answer is computed from it and nothing is kept.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("ServerRoster"), ActionName("ServerRoster")]
        public IActionResult ResidentRosterPage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/ServerRoster.cshtml", new ResidentRosterViewModel());

        [HttpGet("RecordRosterCedar")]
        public IActionResult RecordRosterCedar() => Ok(RecordRosterPageOf("Cedar"));

        [HttpPost("RecordRosterWingResidents")]
        public IActionResult RecordRosterWingResidents([FromBody] RecordRosterWingQuery query) =>
            Ok(new RecordRosterWing { Wing = query.Wing, Page = RecordRosterPageOf(query.Wing) });

        // Rosa Diaz moves into suite 106 today; the roster lists Cedar wing by suite, so she takes 106's place.
        [HttpPost("RecordRosterTodaysArrival")]
        public IActionResult RecordRosterTodaysArrival()
        {
            var arrival = new RecordRosterResident { ResidentId = 6710, ResidentName = "Rosa Diaz", Suite = "106", CareLevel = "Assisted Living" };
            return Ok(new RecordRosterArrival
            {
                Resident = arrival,
                Position = RecordRosterResidents("Cedar").Count(r => string.CompareOrdinal(r.Suite, arrival.Suite) < 0),
                Summary = $"{arrival.ResidentName} moves into suite {arrival.Suite} today."
            });
        }

        // Henry Liu came back from the hospital needing skilled nursing; the record has his new care level.
        [HttpPost("RecordRosterCareLevel")]
        public IActionResult RecordRosterCareLevel()
        {
            var henry = RecordRosterResidents("Cedar").Single(r => r.ResidentId == 6702);
            henry.CareLevel = "Skilled Nursing";
            return Ok(new RecordRosterUpdate
            {
                Resident = henry,
                Summary = $"{henry.ResidentName}'s care level is now {henry.CareLevel} in the record."
            });
        }

        // Grace Bennett's move from suite 108 to the larger suite 118 is approved.
        [HttpPost("RecordRosterSuiteMove")]
        public IActionResult RecordRosterSuiteMove()
        {
            var grace = RecordRosterResidents("Cedar").Single(r => r.ResidentId == 6703);
            var from = grace.Suite;
            grace.Suite = "118";
            return Ok(new RecordRosterUpdate
            {
                Resident = grace,
                Summary = $"{grace.ResidentName} moves from suite {from} to suite {grace.Suite}."
            });
        }

        private static RecordRosterPage RecordRosterPageOf(string wing)
        {
            var residents = RecordRosterResidents(wing);
            return new RecordRosterPage
            {
                Result = residents,
                Summary = $"{residents.Count} residents on the record."
            };
        }

        private static List<RecordRosterResident> RecordRosterResidents(string wing) => wing == "Birch"
            ? new()
            {
                new RecordRosterResident { ResidentId = 6801, ResidentName = "Jonah Reed", Suite = "201", CareLevel = "Independent" },
                new RecordRosterResident { ResidentId = 6802, ResidentName = "Priya Shah", Suite = "203", CareLevel = "Assisted Living" },
                new RecordRosterResident { ResidentId = 6803, ResidentName = "Ruth Carter", Suite = "205", CareLevel = "Memory Care" },
                new RecordRosterResident { ResidentId = 6804, ResidentName = "Samuel Diaz", Suite = "207", CareLevel = "Assisted Living" }
            }
            : new()
            {
                new RecordRosterResident { ResidentId = 6701, ResidentName = "Amina Patel", Suite = "101", CareLevel = "Assisted Living" },
                new RecordRosterResident { ResidentId = 6702, ResidentName = "Henry Liu", Suite = "104", CareLevel = "Assisted Living" },
                new RecordRosterResident { ResidentId = 6703, ResidentName = "Grace Bennett", Suite = "108", CareLevel = "Independent" },
                new RecordRosterResident { ResidentId = 6704, ResidentName = "Walter Grant", Suite = "112", CareLevel = "Memory Care" },
                new RecordRosterResident { ResidentId = 6705, ResidentName = "Edith Hale", Suite = "115", CareLevel = "Assisted Living" }
            };
    }
}
