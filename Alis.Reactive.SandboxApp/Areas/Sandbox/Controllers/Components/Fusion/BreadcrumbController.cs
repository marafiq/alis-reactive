using System;
using System.Collections.Generic;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Care-record navigation journey: a coordinator steps up the breadcrumb trail to
    /// open a higher section of a resident's record. Open resolves the section summary
    /// from the clicked crumb's url and the record code from the clicked crumb's id.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/CareRecordBreadcrumb")]
    public class BreadcrumbController : Controller
    {
        private static readonly IReadOnlyDictionary<string, string> SummaryByUrl =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["/communities/sunrise-court"] = "Sunrise Court is home to 84 residents across 3 neighborhoods.",
                ["/residents/eleanor-hughes"] = "Eleanor Hughes, Room 214, Memory Care since March 2024.",
                ["/residents/eleanor-hughes/care-plan"] = "Care Plan: mobility support twice daily, low-sodium diet, weekly vitals."
            };

        private static readonly IReadOnlyDictionary<string, string> CodeById =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["community"] = "COM-SC",
                ["resident"] = "RES-214",
                ["care-plan"] = "CP-214"
            };

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Breadcrumb/Index.cshtml",
                new FusionBreadcrumbModel());
        }

        [HttpPost("Open")]
        public IActionResult Open([FromBody] OpenCareSectionRequest request)
        {
            return Ok(new OpenCareSectionResponse
            {
                Heading = request.Text,
                Summary = SummaryByUrl.TryGetValue(request.Url, out var summary)
                    ? summary
                    : "That section has no summary on file.",
                SectionCode = CodeById.TryGetValue(request.Id, out var code)
                    ? code
                    : "UNKNOWN"
            });
        }
    }
}
