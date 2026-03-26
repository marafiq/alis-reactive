using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/AutoComplete")]
    public class AutoCompleteController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Physicians = new List<PhysicianItem>
            {
                new() { Value = "smith", Text = "Dr. Smith", Specialty = "Cardiology" },
                new() { Value = "johnson", Text = "Dr. Johnson", Specialty = "Neurology" },
                new() { Value = "williams", Text = "Dr. Williams", Specialty = "Geriatrics" },
                new() { Value = "brown", Text = "Dr. Brown", Specialty = "Internal Medicine" }
            };
            ViewBag.MedicationTypes = new List<MedicationTypeItem>
            {
                new() { Value = "analgesic", Text = "Analgesic", Category = "Pain" },
                new() { Value = "antibiotic", Text = "Antibiotic", Category = "Infection" },
                new() { Value = "antiviral", Text = "Antiviral", Category = "Infection" },
                new() { Value = "steroid", Text = "Steroid", Category = "Inflammation" }
            };
            return View("~/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml", new AutoCompleteModel());
        }

        [HttpGet("Medications")]
        public IActionResult Medications([FromQuery] string? MedicationType)
        {
            var all = new List<MedicationTypeItem>
            {
                new() { Value = "analgesic", Text = "Analgesic", Category = "Pain" },
                new() { Value = "antibiotic", Text = "Antibiotic", Category = "Infection" },
                new() { Value = "antiviral", Text = "Antiviral", Category = "Infection" },
                new() { Value = "antifungal", Text = "Antifungal", Category = "Infection" },
                new() { Value = "steroid", Text = "Steroid", Category = "Inflammation" },
                new() { Value = "statin", Text = "Statin", Category = "Cholesterol" },
                new() { Value = "ssri", Text = "SSRI", Category = "Mental Health" },
                new() { Value = "benzodiazepine", Text = "Benzodiazepine", Category = "Anxiety" }
            };

            // Gather sends the component's selected value (null → "null" string when nothing selected).
            // Treat "null" same as empty — return all medications.
            var search = MedicationType == "null" ? null : MedicationType;
            var filtered = string.IsNullOrEmpty(search)
                ? all
                : all.Where(m => m.Text.Contains(search, StringComparison.OrdinalIgnoreCase)
                              || m.Value.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(new MedicationSearchResponse
            {
                Medications = filtered,
                Count = filtered.Count
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
