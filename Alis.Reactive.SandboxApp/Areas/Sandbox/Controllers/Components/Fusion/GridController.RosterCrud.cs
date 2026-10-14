using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Cedar wing front desk: the desk admits a resident into a free suite, moves a resident to another
    /// suite and records today's move-outs, and the roster follows each answer. Every page starts from the
    /// roster on file; an answer checks the suites on file and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        // Frank Delgado's final invoice is still open, so the business office has not cleared his move-out.
        private const int CedarFinalInvoiceOpenResidentId = 7206;

        [HttpGet("RosterCrud"), ActionName("RosterCrud")]
        public IActionResult CedarFrontDeskPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/RosterCrud.cshtml",
                new RosterCrudModel { MovingOutToday = CedarMovingOutToday() });

        [HttpGet("RosterCrudRoster")]
        public IActionResult RosterCrudRoster() => Ok(CedarRosterOnFile());

        [HttpPost("RosterCrudAdmit")]
        public IActionResult RosterCrudAdmit([FromBody] RosterCrudModel? admission)
        {
            var name = admission?.AdmitName?.Trim() ?? "";
            var suite = admission?.AdmitSuite?.Trim() ?? "";
            if (name.Length == 0)
                return BadRequest(new CedarDeskRefusal { Summary = "Write the new resident's name." });
            if (suite.Length == 0)
                return BadRequest(new CedarDeskRefusal { Summary = $"Choose the suite {name} moves into." });

            var roster = CedarRosterOnFile();
            var occupant = roster.FirstOrDefault(resident => resident.Suite == suite);
            if (occupant != null)
                return BadRequest(new CedarDeskRefusal { Summary = $"Suite {suite} is {occupant.ResidentName}'s. Choose another suite." });

            return Ok(new CedarAdmitted
            {
                Resident = new CedarResident { ResidentId = 7207, ResidentName = name, Suite = suite, MovedIn = "Oct 14, 2026" },
                // The roster runs in suite order; the new resident follows every lower suite.
                Position = roster.Count(resident => string.CompareOrdinal(resident.Suite, suite) < 0),
                Summary = $"{name} is admitted to suite {suite}."
            });
        }

        [HttpPost("RosterCrudMove")]
        public IActionResult RosterCrudMove([FromBody] CedarSuiteChange? change)
        {
            var moving = change?.Residents?.FirstOrDefault();
            if (moving == null)
                return BadRequest(new CedarDeskRefusal { Summary = "Pick the resident who is moving in the roster." });

            var suite = change!.NewSuite?.Trim() ?? "";
            if (suite.Length == 0)
                return BadRequest(new CedarDeskRefusal { Summary = $"Choose the suite {moving.ResidentName} moves to." });

            var occupant = CedarRosterOnFile().FirstOrDefault(resident => resident.Suite == suite && resident.ResidentId != moving.ResidentId);
            if (occupant != null)
                return BadRequest(new CedarDeskRefusal { Summary = $"Suite {suite} is {occupant.ResidentName}'s. Choose another suite." });

            moving.Suite = suite;
            return Ok(new CedarResidentMoved { Resident = moving, Summary = $"{moving.ResidentName} moves to suite {suite}." });
        }

        [HttpPost("RosterCrudMoveOut")]
        public IActionResult RosterCrudMoveOut([FromBody] CedarMoveOut? moveOut)
        {
            var leaving = CedarMovingOutToday().FirstOrDefault(resident => resident.ResidentId == moveOut?.ResidentId);
            if (leaving == null)
                return BadRequest(new CedarDeskRefusal { Summary = "That resident is not moving out today." });
            if (leaving.ResidentId == CedarFinalInvoiceOpenResidentId)
                return Conflict(new CedarDeskRefusal
                {
                    Summary = $"{leaving.ResidentName}'s final invoice is still open; the business office closes it before the move-out."
                });

            return Ok(new CedarMovedOut
            {
                ResidentId = leaving.ResidentId,
                Summary = $"{leaving.ResidentName} has moved out of suite {leaving.Suite}."
            });
        }

        private static List<CedarResident> CedarRosterOnFile() => new()
        {
            new CedarResident { ResidentId = 7201, ResidentName = "Agnes Whitfield", Suite = "101", MovedIn = "Mar 2, 2021" },
            new CedarResident { ResidentId = 7202, ResidentName = "Bernard Okafor", Suite = "104", MovedIn = "Jun 18, 2022" },
            new CedarResident { ResidentId = 7203, ResidentName = "Clara Jensen", Suite = "108", MovedIn = "Jan 9, 2023" },
            new CedarResident { ResidentId = 7204, ResidentName = "Dennis Moreau", Suite = "112", MovedIn = "Aug 30, 2023" },
            new CedarResident { ResidentId = 7205, ResidentName = "Eleanor Price", Suite = "116", MovedIn = "Apr 4, 2024" },
            new CedarResident { ResidentId = 7206, ResidentName = "Frank Delgado", Suite = "120", MovedIn = "Feb 11, 2025" }
        };

        private static List<CedarMovingOut> CedarMovingOutToday() => new()
        {
            new CedarMovingOut { ResidentId = 7204, ResidentName = "Dennis Moreau", Suite = "112" },
            new CedarMovingOut { ResidentId = 7206, ResidentName = "Frank Delgado", Suite = "120" }
        };
    }
}
