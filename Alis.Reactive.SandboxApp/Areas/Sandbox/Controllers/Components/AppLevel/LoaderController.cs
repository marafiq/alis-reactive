using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Care plan review: refreshing the census answers at once, while saving the care plan takes
    /// the server a moment, so the page's loader must stay up until the save answers.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Loader")]
    public class LoaderController : Controller
    {
        private static readonly TimeSpan CarePlanSaveDuration = TimeSpan.FromSeconds(3);

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/AppLevel/Loader/Index.cshtml", new CarePlanReviewModel());

        [HttpGet("Census")]
        public IActionResult Census() => Ok(new CensusSummary("42 residents in today's census."));

        [HttpPost("SaveCarePlan")]
        public async Task<IActionResult> SaveCarePlan([FromBody] CarePlanReviewModel? carePlan)
        {
            if (carePlan == null) return BadRequest();

            await Task.Delay(CarePlanSaveDuration, HttpContext.RequestAborted);
            return Ok(new CarePlanSaved("Care plan saved."));
        }
    }
}
