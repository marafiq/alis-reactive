using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionRadioButton")]
    public sealed class RadioButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/RadioButton/Index.cshtml", new FusionRadioButtonModel());
        }

        [HttpPost("ConfirmPlan")]
        public IActionResult ConfirmPlan([FromBody] RoomPlanRequest request)
        {
            return Ok(new RoomPlanResponse
            {
                Confirmation = BuildConfirmation(request)
            });
        }

        private static string BuildConfirmation(RoomPlanRequest request)
        {
            if (request.CompanionSuiteChosen && request.CompanionSuiteUnavailable)
            {
                return "Companion suite is full this month — please choose another room before confirming.";
            }

            return $"Move-in confirmed: {request.Room}.";
        }
    }
}
