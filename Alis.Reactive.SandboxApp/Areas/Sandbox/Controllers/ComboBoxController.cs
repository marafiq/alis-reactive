using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ComboBoxController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Physicians = new List<string> { "Dr. Smith", "Dr. Johnson", "Dr. Williams", "Dr. Brown" };
            ViewBag.MedicationTypes = new List<string> { "Analgesic", "Antibiotic", "Antiviral", "Steroid" };
            return View(new ComboBoxModel
            {
                Physician = null,
                MedicationType = null
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
