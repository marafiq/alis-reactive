using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.AllModulesTogether.CrossVendor.ComponentApi;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.CrossVendor
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/TestWidget")]
    public class TestWidgetController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index() => View("~/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml", new TestWidgetModel());

        [HttpGet("DataSource")]
        public IActionResult DataSource() => Ok(new
        {
            name = "Widget Data",
            count = 3,
            selected = "beta",
            items = new[] { "alpha", "beta", "gamma" },
            detail = new
            {
                region = "US-East",
                metadata = new { version = 2 }
            }
        });

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data) => Ok(data);
    }
}
