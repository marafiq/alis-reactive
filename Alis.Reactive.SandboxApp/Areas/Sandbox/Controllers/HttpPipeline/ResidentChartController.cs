using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ResidentChart")]
    public class ResidentChartController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/HttpPipeline/ResidentChart/Index.cshtml",
                new ResidentChartPageModel { ReviewChartMrn = "MRN-20417" });

        [HttpGet("Floor")]
        public IActionResult Floor()
            => Ok(new FloorCharts(new[]
            {
                new ResidentChart("Helen Park", "MRN-20417"),
                new ResidentChart("Arthur Bell", null),
                new ResidentChart("Rosa Diaz", "MRN-20533"),
            }));

        [HttpGet("Chart/{resident}")]
        public IActionResult Chart(string resident) => resident switch
        {
            "helen" => Ok(new ResidentChart("Helen Park", "MRN-20417")),
            "arthur" => Ok(new ResidentChart("Arthur Bell", null)),
            "rosa" => Ok(new ResidentChart("Rosa Diaz", "MRN-20533")),
            _ => NotFound(),
        };
    }
}
