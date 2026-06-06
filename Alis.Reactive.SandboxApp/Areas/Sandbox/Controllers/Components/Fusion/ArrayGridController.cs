using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// ArrayGrid sandbox — the array DSL routed into a component data source. A roster loads once
    /// over HTTP; a client-side <c>ReactiveArray</c> transform (filter/sort over element members)
    /// feeds the grid via <c>SetDataSource(TypedSource&lt;T[]&gt;)</c>, and re-filtering the rows
    /// already on screen reads the grid's own <c>dataSource</c> member — no HTTP round-trip.
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

        /// <summary>Returns the resident object array transformed before grid binding.</summary>
        [HttpGet("Residents")]
        public IActionResult Residents()
        {
            return Ok(new ResidentRosterResponse
            {
                Residents = new[]
                {
                    new ResidentRow { Name = "Ada", Status = "active",     Age = 71, Balance = 1200 },
                    new ResidentRow { Name = "Bo",  Status = "discharged", Age = 64, Balance = 500 },
                    new ResidentRow { Name = "Cy",  Status = "active",     Age = 80, Balance = 2000 },
                    new ResidentRow { Name = "Di",  Status = "critical",   Age = 90, Balance = 3000 },
                    new ResidentRow { Name = "Ed",  Status = "active",     Age = 55, Balance = 800 },
                },
            });
        }
    }
}
