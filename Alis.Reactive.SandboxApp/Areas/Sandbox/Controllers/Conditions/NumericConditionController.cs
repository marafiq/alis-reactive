using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/NumericCondition")]
public class NumericConditionController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View("~/Areas/Sandbox/Views/Conditions/NumericCondition/Index.cshtml", new NumericConditionModel
        {
            HeartRate = 0,
            BloodPressure = 0,
            ThresholdValue = 100
        });
    }
}
