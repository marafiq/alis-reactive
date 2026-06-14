using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Carousel")]
    public class CarouselController : Controller
    {
        private static readonly string[] SectionNames =
        {
            "Medications",
            "Therapy Goals",
            "Discharge Steps"
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Carousel/Index.cshtml",
                new FusionCarouselModel());
        }

        [HttpPost("Record")]
        public IActionResult Record([FromBody] CarePlanReviewEntry entry)
        {
            var section = SectionName(entry.SectionIndex);
            var cameFrom = SectionName(entry.CameFromIndex);
            var movement = entry.Direction == "Previous" ? "went back to" : "moved forward to";
            var navigatedBy = entry.BySwipe ? "by swiping" : "using the buttons";

            return Ok(new CarePlanReviewResponse
            {
                Section = section,
                CameFrom = cameFrom,
                Movement = movement,
                NavigatedBy = navigatedBy,
                ChartLine = $"Recorded: {movement} {section} (from {cameFrom}), {navigatedBy}."
            });
        }

        private static string SectionName(int index) =>
            index >= 0 && index < SectionNames.Length ? SectionNames[index] : "an unknown section";
    }
}
