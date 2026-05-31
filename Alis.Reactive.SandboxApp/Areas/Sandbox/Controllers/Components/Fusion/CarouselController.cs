using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionCarousel")]
    public class CarouselController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Carousel/Index.cshtml",
                new FusionCarouselModel());
        }

        [HttpPost("Audit")]
        public IActionResult Audit([FromBody] FusionCarouselAuditRequest request)
        {
            var section = request.CurrentIndex switch
            {
                0 => "vitals snapshot",
                1 => "care plan review",
                2 => "discharge readiness",
                _ => "unknown section"
            };

            return Ok(new FusionCarouselAuditResponse
            {
                Section = section,
                Direction = request.SlideDirection,
                Message = $"Saved review slide {request.CurrentIndex}: {section}"
            });
        }
    }
}
