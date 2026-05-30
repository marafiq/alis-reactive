using Alis.Reactive.Native.Components;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// ArrayOps sandbox — demonstrates the deterministic array-operations DSL:
    /// a NativeCheckList Changed event delivers e.Value (string[]); the plan counts it
    /// via an array-op node and writes the result to a display element. URL:
    /// /Sandbox/Components/ArrayOps.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ArrayOps")]
    public class ArrayOpsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.ActivityItems = new[]
            {
                new RadioButtonItem("PhysicalTherapy", "Physical Therapy"),
                new RadioButtonItem("OccupationalTherapy", "Occupational Therapy"),
                new RadioButtonItem("SpeechTherapy", "Speech Therapy"),
                new RadioButtonItem("SocialActivities", "Social Activities"),
                new RadioButtonItem("DiningProgram", "Dining Program"),
            };

            return View(
                "~/Areas/Sandbox/Views/Components/Native/ArrayOps/Index.cshtml",
                new ArrayOpsModel());
        }

        /// <summary>Resident roster the array DSL operates on (the object array source).</summary>
        [HttpGet("Residents")]
        public IActionResult Residents()
        {
            return Ok(new ResidentRosterResponse
            {
                Residents = new[]
                {
                    new ResidentRow { Name = "Ada",  Status = "active",     Age = 71, Balance = 1200 },
                    new ResidentRow { Name = "Bo",   Status = "discharged", Age = 64, Balance = 500 },
                    new ResidentRow { Name = "Cy",   Status = "active",     Age = 80, Balance = 2000 },
                    new ResidentRow { Name = "Di",   Status = "critical",   Age = 90, Balance = 3000 },
                    new ResidentRow { Name = "Ed",   Status = "active",     Age = 55, Balance = 800 },
                },
            });
        }
    }
}
