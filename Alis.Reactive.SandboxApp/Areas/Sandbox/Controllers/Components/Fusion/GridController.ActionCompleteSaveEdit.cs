using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Pharmacy name corrections: a name corrected on the Maple wing roster goes to Riverside Pharmacy, so
    /// the labels on the resident's medications match. Every page starts from the roster on file; the
    /// pharmacy's answer says what it was told, and nothing is kept.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("ActionCompleteSaveEditRoster")]
        public IActionResult ActionCompleteSaveEditRoster() => Ok(PharmacyRosterOnFile());

        [HttpPost("ActionCompleteSaveEditNotice")]
        public IActionResult ActionCompleteSaveEditNotice([FromBody] PharmacyNameCorrection? correction)
        {
            correction ??= new PharmacyNameCorrection();

            // The printed roster numbers its lines from one.
            var line = correction.RosterLine is int index ? (index + 1).ToString() : "unknown";
            return Ok(new PharmacyNotice
            {
                Summary = $"Riverside Pharmacy has the corrected name: {correction.PreviousName} is now {correction.CorrectedName} (Maple roster line {line})."
            });
        }

        // Two names were keyed in wrong at move-in; the front desk has the right spelling.
        private static List<PharmacyRosterResident> PharmacyRosterOnFile() => new()
        {
            new PharmacyRosterResident { ResidentId = 6401, ResidentName = "Amina Patel", Suite = "101", PrimaryNurse = "Nora Ellis" },
            new PharmacyRosterResident { ResidentId = 6402, ResidentName = "Grace Benett", Suite = "104", PrimaryNurse = "Malik Stone" },
            new PharmacyRosterResident { ResidentId = 6403, ResidentName = "Henry Lui", Suite = "108", PrimaryNurse = "Elena Ruiz" },
            new PharmacyRosterResident { ResidentId = 6404, ResidentName = "Irene Morgan", Suite = "112", PrimaryNurse = "Owen Park" }
        };
    }
}
