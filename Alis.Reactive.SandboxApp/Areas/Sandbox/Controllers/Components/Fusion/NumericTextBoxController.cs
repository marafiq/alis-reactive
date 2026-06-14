using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/NumericTextBox")]
    public class NumericTextBoxController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            // The resident's plan carries over from last month: 7 meals/week, 2 wellness checks/week.
            return View("~/Areas/Sandbox/Views/Components/Fusion/NumericTextBox/Index.cshtml", new NumericTextBoxModel
            {
                MealsPerWeek = 7,
                WellnessChecksPerWeek = 2
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ServicePlanRequest request)
        {
            return Ok(new ServicePlanResponse
            {
                MealsPerWeek = request.MealsPerWeek,
                Summary = "Saved. This resident will receive " + request.MealsPerWeek
                    + " catered meals each week."
            });
        }
    }
}
