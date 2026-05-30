using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Slider")]
    public class SliderController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Slider/Index.cshtml", new SliderModel
            {
                PainScore = 25,
                PreferredRange = [20, 60]
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] SliderEchoRequest request)
        {
            return Ok(new SliderEchoResponse
            {
                PainScore = request.PainScore,
                PreferredRange = request.PreferredRange,
                Summary = request.PainScore + ":" + string.Join(",", request.PreferredRange)
            });
        }
    }
}
