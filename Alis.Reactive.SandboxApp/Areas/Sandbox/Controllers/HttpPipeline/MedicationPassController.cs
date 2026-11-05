using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/MedicationPass")]
    public class MedicationPassController : Controller
    {
        private static readonly IReadOnlyDictionary<int, string> Residents = new Dictionary<int, string>
        {
            [1042] = "Helen Park",
            [1043] = "Arthur Lee",
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.Residents = Residents;
            var pass = new MedicationPassModel { Administered = Residents.Keys.ToDictionary(residentId => residentId, _ => false) };
            return View("~/Areas/Sandbox/Views/HttpPipeline/MedicationPass/Index.cshtml", pass);
        }

        [HttpPost("Record")]
        public IActionResult Record([FromBody] MedicationPassModel? pass)
        {
            if (pass == null) return BadRequest();

            return Ok(new MedicationPassConfirmation(ConfirmationFor(pass), "Next pass at 8:00 PM."));
        }

        [HttpGet("Refills")]
        public IActionResult Refills() => Ok(new RefillOrder("Refills ordered from Maple Pharmacy."));

        private static string ConfirmationFor(MedicationPassModel pass)
        {
            var given = pass.Administered
                .Where(dose => dose.Value)
                .Select(dose => Residents[dose.Key])
                .ToList();

            return given.Count == 0
                ? "No doses recorded."
                : $"Doses recorded for {string.Join(", ", given)}.";
        }
    }
}
