using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>Discharge planning: the plan's rules depend on its own answers, on the page and on the server alike.</summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/DischargePlanning")]
    public class DischargePlanningController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/DischargePlanning/Index.cshtml", new DischargePlanModel());

        [HttpPost("Save")]
        public IActionResult Save([FromBody] DischargePlanModel? plan)
        {
            if (plan == null) return BadRequest();

            var result = new DischargePlanValidator().Validate(plan);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new DischargePlanConfirmation($"Discharge plan saved for {plan.ResidentName}."));
        }
    }
}
