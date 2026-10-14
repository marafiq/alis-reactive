using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Resident roster: the front desk's roster loads once and is sorted by name in the browser; narrowing
    /// it to the active residents reads the rows already on screen, with no second request.
    /// URL: /Sandbox/Components/ArrayGrid.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ArrayGrid")]
    public class ArrayGridController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/ArrayGrid/Index.cshtml",
                new ArrayGridModel());
        }

        /// <summary>The roster, in no particular order: the page sorts it.</summary>
        [HttpGet("Residents")]
        public IActionResult Residents()
        {
            return Ok(new RosterResponse
            {
                Residents = new[]
                {
                    new RosterResident { Name = "Bo",  Status = "discharged", Age = 64, Balance = 500 },
                    new RosterResident { Name = "Di",  Status = "critical",   Age = 90, Balance = 3000 },
                    new RosterResident { Name = "Ada", Status = "active",     Age = 71, Balance = 1200 },
                    new RosterResident { Name = "Ed",  Status = "active",     Age = 55, Balance = 800 },
                    new RosterResident { Name = "Cy",  Status = "active",     Age = 80, Balance = 2000 },
                },
            });
        }
    }
}
