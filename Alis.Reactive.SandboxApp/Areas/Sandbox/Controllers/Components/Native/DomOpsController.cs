using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.DomOps;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Lunch seating: the dining supervisor checks each table's needs before service, read off the table
    /// cards the page renders. URL: /Sandbox/Components/DomOps.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/DomOps")]
    public class DomOpsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Native/DomOps/Index.cshtml", new LunchSeatingModel());
    }
}
