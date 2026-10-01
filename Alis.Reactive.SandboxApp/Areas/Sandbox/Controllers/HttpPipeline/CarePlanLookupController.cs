using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Care plan lookup: Helen Park (1042) and Arthur Bell (1043) each have a care plan; Rosa Diaz (1044),
    /// admitted this week, has none on file yet.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/CarePlanLookup")]
    public class CarePlanLookupController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/HttpPipeline/CarePlanLookup/Index.cshtml", new CarePlanLookupModel());

        [HttpGet("CarePlan/{residentId:int}")]
        public IActionResult CarePlan(int residentId) => residentId switch
        {
            1042 => Ok(new CarePlanSummary("Helen Park", "Assisted living: morning medication round, physiotherapy on Tuesdays and Fridays.")),
            1043 => Ok(new CarePlanSummary("Arthur Bell", "Memory care: escorted meals, evening check every two hours.")),
            _ => NotFound(),
        };
    }
}
