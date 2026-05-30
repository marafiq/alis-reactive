using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// DomOps sandbox — the array DSL over native DOM. A DOM element resolved by getElementById
    /// is a JS object; its classList/children are array-likes the runtime normalizes, so the same
    /// array ops apply. URL: /Sandbox/Components/DomOps.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/DomOps")]
    public class DomOpsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Native/DomOps/Index.cshtml", new DomOpsModel());
    }
}
