using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Medication reconciliation: Save takes a reconciliation the page checked first; Print the MAR takes
    /// it as it is and runs the same validator.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/MedicationReconciliation")]
    public class MedicationReconciliationController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var reconciliation = new MedicationReconciliationModel
            {
                Medications = { new MedicationLine(), new MedicationLine() },
                Allergies = { "", "" },
            };
            return View("~/Areas/Sandbox/Views/Validation/MedicationReconciliation/Index.cshtml", reconciliation);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] MedicationReconciliationModel? reconciliation) =>
            Accept(reconciliation, "Reconciliation saved");

        [HttpPost("Print")]
        public IActionResult Print([FromBody] MedicationReconciliationModel? reconciliation) =>
            Accept(reconciliation, "MAR printed");

        private IActionResult Accept(MedicationReconciliationModel? reconciliation, string outcome)
        {
            if (reconciliation == null) return BadRequest();

            var result = new MedicationReconciliationValidator().Validate(reconciliation);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new MedicationReconciliationConfirmation($"{outcome} for {reconciliation.ResidentName}."));
        }
    }
}
