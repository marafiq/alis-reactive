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
    }
}
