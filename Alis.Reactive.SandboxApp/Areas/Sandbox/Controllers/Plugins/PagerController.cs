using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.Pager;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Plugins
{
    [Area("Sandbox")]
    [Route("Sandbox/Plugins/Pager")]
    public class PagerController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Plugins/Pager/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new WingPagerModel());

        [HttpGet("Duplicate")]
        public IActionResult Duplicate() => View(ViewBase + "Duplicate.cshtml", new WingPagerModel());
    }
}
