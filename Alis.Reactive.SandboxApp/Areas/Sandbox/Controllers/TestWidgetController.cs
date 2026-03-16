using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class TestWidgetController : Controller
    {
        public IActionResult Index() => View(new Models.TestWidgetModel());

        [HttpGet]
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

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data) => Ok(data);
    }
}
