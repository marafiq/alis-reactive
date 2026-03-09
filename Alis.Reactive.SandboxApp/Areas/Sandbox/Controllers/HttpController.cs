using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class HttpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Residents()
        {
            return Json(new[]
            {
                new { name = "John Doe", age = 82 },
                new { name = "Jane Smith", age = 75 }
            });
        }

        [HttpPost]
        public IActionResult Save([FromBody] SaveRequest? request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return BadRequest(new { errors = new { Name = new[] { "Name is required." } } });
            }

            return Ok(new { message = $"Saved: {request.Name}" });
        }

        [HttpGet]
        public IActionResult Facilities()
        {
            return Json(new[]
            {
                new { id = 1, name = "Main Campus" },
                new { id = 2, name = "West Wing" }
            });
        }

        public class SaveRequest
        {
            public string? Name { get; set; }
        }
    }
}
