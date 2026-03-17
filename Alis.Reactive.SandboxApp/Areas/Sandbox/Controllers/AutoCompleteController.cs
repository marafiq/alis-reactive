using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class AutoCompleteController : Controller
    {
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
            return View(new AutoCompleteModel());
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
