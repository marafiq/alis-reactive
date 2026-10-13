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
            // What this nurse gave recently; the field offers it until the pharmacy search answers.
            ViewBag.RecentMedications = new List<MedicationTypeItem>
            {
                new() { Value = "sterile-saline-flush", Text = "Sterile saline flush", Category = "Recent" },
                new() { Value = "acetaminophen", Text = "Acetaminophen", Category = "Recent" }
            };
            return View("~/Areas/Sandbox/Views/Components/Fusion/AutoComplete/Index.cshtml", new AutoCompleteModel());
        }

        // The pharmacy searches one formulary: the assisted living formulary carries no antifungal and
        // no benzodiazepine. A search that names no known formulary is refused.
        [HttpGet("Medications")]
        public IActionResult Medications([FromQuery] MedicationSearchQuery query)
        {
            if (query.Formulary != "assisted-living") return BadRequest();

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

            var formulary = all.Where(m => m.Value != "antifungal" && m.Value != "benzodiazepine").ToList();

            // Filtering gather can serialize an empty selection as "null"; treat it like no search text.
            var search = query.MedicationType == "null" ? null : query.MedicationType;
            var filtered = string.IsNullOrEmpty(search)
                ? formulary
                : formulary.Where(m => m.Text.Contains(search, StringComparison.OrdinalIgnoreCase)
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
