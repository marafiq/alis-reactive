using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ComboBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The Maple wing's 08:00 medication pass: the page that picks the resident, the chart it shows, and the
    /// record of the medications given.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionComboBox")]
    public sealed class ComboBoxController : Controller
    {
        // The round, in the order the nurse walks it, with each resident's chart.
        private static readonly IReadOnlyList<ChartedResident> Round =
        [
            new("R-1031", "Bell, Arthur", "Maple 202", "No known allergies"),
            new("R-1042", "Hale, Margaret", "Maple 214", "Penicillin (hives)"),
            new("R-1057", "Okafor, Grace", "Maple 221", "Sulfa drugs (rash)"),
            new("R-1063", "Russo, Dominic", "Maple 230", "Latex")
        ];

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/ComboBox/Index.cshtml", new FusionComboBoxModel
            {
                Round = Round.Select(resident => new RoundResident { Id = resident.Id, Name = resident.Name }).ToList()
            });

        [HttpGet("Chart")]
        public IActionResult Chart([FromQuery] DoseCheckQuery query)
        {
            var resident = Find(query.ResidentId);
            return resident is null
                ? NotFound()
                : Ok(new DoseCheck { Room = resident.Room, Allergies = resident.Allergies });
        }

        [HttpPost("Record")]
        public IActionResult Record([FromBody] MedicationPassRecord? record)
        {
            var resident = Find(record?.Resident);
            if (resident is null)
                return BadRequest(new MedicationPassStatus { Message = "Choose a resident before recording medications." });

            return Ok(new MedicationPassStatus
            {
                Message = $"Recorded the 08:00 medications for {resident.Name}, resident {record!.RoundPosition + 1} of {Round.Count} on the round."
            });
        }

        private static ChartedResident? Find(string? residentId) =>
            Round.FirstOrDefault(resident => resident.Id == residentId);

        private sealed record ChartedResident(string Id, string Name, string Room, string Allergies);
    }
}
