using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
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
