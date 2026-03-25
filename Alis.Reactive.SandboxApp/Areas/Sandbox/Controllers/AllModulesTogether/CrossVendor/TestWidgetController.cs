using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.CrossVendor
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/TestWidget")]
    public class TestWidgetController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/AllModulesTogether/TestWidget/Index.cshtml");
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }

        [HttpGet("DataSource")]
        public IActionResult DataSource()
        {
            return Ok(new { value = "beta", items = new[] { "alpha", "beta", "gamma" } });
        }
    }
}
