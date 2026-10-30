using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Plugins.Pager;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Plugins
{
    /// <summary>
    /// The wing's pager: a plugin with every kind of member, and a plugin that is itself a function.
    /// URL: /Sandbox/Plugins/Pager.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Plugins/Pager")]
    public class PagerController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Plugins/Pager/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new WingPagerModel());

        // A view that registers the pager twice: the plan refuses it as the view renders.
        [HttpGet("Duplicate")]
        public IActionResult Duplicate() => View(ViewBase + "Duplicate.cshtml", new WingPagerModel());
    }
}
