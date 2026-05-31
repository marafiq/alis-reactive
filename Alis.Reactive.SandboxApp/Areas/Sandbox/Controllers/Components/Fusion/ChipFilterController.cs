using System;
using System.Linq;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// ChipFilter sandbox — multi-select chips drive the array DSL and a server-side grid filter.
    /// The chip selection is broadcast as a custom event whose payload carries the selected chip
    /// objects; the array DSL counts/guards them by member, and their texts gather into a POST that
    /// filters the resident grid. URL: /Sandbox/Components/ChipFilter.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ChipFilter")]
    public class ChipFilterController : Controller
    {
        private static readonly CareResident[] All =
        {
            new CareResident { Name = "Ada", CareLevel = "Memory Care" },
            new CareResident { Name = "Bo",  CareLevel = "Assisted" },
            new CareResident { Name = "Cy",  CareLevel = "Memory Care" },
            new CareResident { Name = "Di",  CareLevel = "Skilled Nursing" },
            new CareResident { Name = "Ed",  CareLevel = "Independent" },
            new CareResident { Name = "Fay", CareLevel = "Assisted" },
            new CareResident { Name = "Gus", CareLevel = "Memory Care" },
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/ChipFilter/Index.cshtml",
                new ChipFilterModel());
        }

        /// <summary>The full roster shown on load.</summary>
        [HttpGet("Residents")]
        public IActionResult Residents() => Ok(new CareResidentResponse { Residents = All });

        /// <summary>Filters the roster by the selected care levels (the chip texts).</summary>
        [HttpPost("Filter")]
        public IActionResult Filter([FromBody] CareFilterRequest? request)
        {
            var levels = request?.CareLevels ?? Array.Empty<string>();
            var residents = levels.Length == 0
                ? All
                : All.Where(r => levels.Contains(r.CareLevel)).ToArray();
            return Ok(new CareResidentResponse { Residents = residents });
        }
    }
}
