using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Loader")]
    public class LoaderController : Controller
    {
        private static readonly TimeSpan CarePlanSaveDuration = TimeSpan.FromSeconds(3);

        [HttpGet("")]
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/Components/AppLevel/Loader/Index.cshtml", new CarePlanReviewModel());

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
