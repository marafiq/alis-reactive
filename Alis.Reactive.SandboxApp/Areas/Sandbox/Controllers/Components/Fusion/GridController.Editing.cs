using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Maple wing roster: the coordinator corrects, admits and moves out residents. Every page starts from
    /// the roster on file; an admission answers with the next resident on the waiting list, and a handoff
    /// correction answers with the resident's corrected row. Nothing is kept between requests.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("EditingRoster")]
        public IActionResult EditingRoster() => Ok(MapleRosterOnFile());

        [HttpPost("EditingAdmission")]
        public IActionResult EditingAdmission() =>
            Ok(new MapleRosterResident
            {
                ResidentId = 6107,
                ResidentName = "Rosa Diaz",
                CareLevel = "Assisted Living",
                Suite = "122",
                RiskLevel = "Moderate",
                OpenTasks = 3,
                PrimaryNurse = "Elena Ruiz"
            });

        [HttpPost("EditingCorrection")]
        public IActionResult EditingCorrection([FromBody] ResidentGridEditingModel? correction)
        {
            correction ??= new ResidentGridEditingModel();
            var result = new MapleRosterCorrectionValidator().Validate(correction);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var resident = MapleRosterOnFile().FirstOrDefault(r =>
                string.Equals(r.ResidentName, correction.ResidentName!.Trim(), StringComparison.OrdinalIgnoreCase));
            if (resident == null)
                return BadRequest(new
                {
                    errors = new Dictionary<string, string[]>
                    {
                        [nameof(ResidentGridEditingModel.ResidentName)] = [$"No resident named {correction.ResidentName} lives on Maple wing."]
                    }
                });

            resident.RiskLevel = correction.RiskLevel!;
            resident.OpenTasks = (int)correction.OpenTasks!.Value;
            return Ok(new MapleRosterCorrected
            {
                Resident = resident,
                Summary = $"{resident.ResidentName}'s row now shows {resident.RiskLevel} risk and {resident.OpenTasks} open tasks."
            });
        }

        private static List<MapleRosterResident> MapleRosterOnFile() => new()
        {
            new MapleRosterResident { ResidentId = 6101, ResidentName = "Amina Patel", CareLevel = "Assisted Living", Suite = "101", RiskLevel = "Low", OpenTasks = 2, PrimaryNurse = "Nora Ellis" },
            new MapleRosterResident { ResidentId = 6102, ResidentName = "Grace Bennett", CareLevel = "Assisted Living", Suite = "104", RiskLevel = "Moderate", OpenTasks = 3, PrimaryNurse = "Malik Stone" },
            new MapleRosterResident { ResidentId = 6103, ResidentName = "Henry Liu", CareLevel = "Assisted Living", Suite = "108", RiskLevel = "Moderate", OpenTasks = 6, PrimaryNurse = "Elena Ruiz" },
            // Irene Morgan's transfer to memory care on Willow wing is booked; she moves on Friday.
            new MapleRosterResident { ResidentId = 6104, ResidentName = "Irene Morgan", CareLevel = "Memory Care", Suite = "112", RiskLevel = "High", OpenTasks = 4, PrimaryNurse = "Owen Park" },
            new MapleRosterResident { ResidentId = 6105, ResidentName = "Jonah Reed", CareLevel = "Independent", Suite = "115", RiskLevel = "Low", OpenTasks = 1, PrimaryNurse = "Nora Ellis" },
            new MapleRosterResident { ResidentId = 6106, ResidentName = "Katherine Ortiz", CareLevel = "Assisted Living", Suite = "118", RiskLevel = "High", OpenTasks = 5, PrimaryNurse = "Malik Stone" }
        };
    }
}
