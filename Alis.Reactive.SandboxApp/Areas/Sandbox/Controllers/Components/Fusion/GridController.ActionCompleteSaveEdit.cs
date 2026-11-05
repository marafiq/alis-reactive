using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    public partial class GridController
    {
        [HttpGet("ActionCompleteSaveEditRoster")]
        public IActionResult ActionCompleteSaveEditRoster() => Ok(PharmacyRosterOnFile());

        [HttpPost("ActionCompleteSaveEditNotice")]
        public IActionResult ActionCompleteSaveEditNotice([FromBody] PharmacyNameCorrection? correction)
        {
            correction ??= new PharmacyNameCorrection();

            var line = correction.RosterLine is int index ? (index + 1).ToString() : "unknown";
            return Ok(new PharmacyNotice
            {
                Summary = $"Riverside Pharmacy has the corrected name: {correction.PreviousName} is now {correction.CorrectedName} (Maple roster line {line})."
            });
        }

        private static List<PharmacyRosterResident> PharmacyRosterOnFile() => new()
        {
            new PharmacyRosterResident { ResidentId = 6401, ResidentName = "Amina Patel", Suite = "101", PrimaryNurse = "Nora Ellis" },
            new PharmacyRosterResident { ResidentId = 6402, ResidentName = "Grace Benett", Suite = "104", PrimaryNurse = "Malik Stone" },
            new PharmacyRosterResident { ResidentId = 6403, ResidentName = "Henry Lui", Suite = "108", PrimaryNurse = "Elena Ruiz" },
            new PharmacyRosterResident { ResidentId = 6404, ResidentName = "Irene Morgan", Suite = "112", PrimaryNurse = "Owen Park" }
        };
    }
}
