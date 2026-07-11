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
                RoomTemperature = 68,
                QuietHours = [13, 15]
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] ComfortPreferencesRequest request)
        {
            var start = request.QuietHours.Length > 0 ? request.QuietHours[0] : 0;
            var end = request.QuietHours.Length > 1 ? request.QuietHours[1] : 0;
            return Ok(new ComfortPreferencesResponse
            {
                Summary = "Saved. We'll keep your room at " + request.RoomTemperature
                    + "°F and hold non-urgent visits from " + start + ":00 to " + end + ":00."
            });
        }
    }
}
