using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Plugins
{
    [Area("Sandbox")]
    [Route("Sandbox/Plugins/ArrayManager")]
    public class ArrayManagerController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Plugins/ArrayManager/Index.cshtml", new ArrayManagerModel());

        [HttpGet("Residents")]
        public IActionResult Residents() => Json(new
        {
            items = new[]
            {
                new { id = 1, name = "John Doe", status = "active", age = 82 },
                new { id = 2, name = "Jane Smith", status = "active", age = 75 },
                new { id = 3, name = "Bob Johnson", status = "discharged", age = 68 },
                new { id = 4, name = "Alice Brown", status = "active", age = 91 },
                new { id = 5, name = "Charlie Wilson", status = "pending", age = 77 },
            }
        });

        // The memory-care ward: three residents, one of them critical.
        [HttpGet("MemoryWard")]
        public IActionResult MemoryWard() => Json(new
        {
            items = new[]
            {
                new { id = 11, name = "Edith Lane", status = "active", age = 88 },
                new { id = 12, name = "Walter Cobb", status = "critical", age = 91 },
                new { id = 13, name = "Iris Moreno", status = "active", age = 84 },
            }
        });

        [HttpGet("PluginEcho")]
        public IActionResult PluginEcho(int? count) => Json(new
        {
            receivedCount = count,
            receivedHeader = Request.Headers["X-Array-Count"].FirstOrDefault() ?? "(none)"
        });
    }
}
