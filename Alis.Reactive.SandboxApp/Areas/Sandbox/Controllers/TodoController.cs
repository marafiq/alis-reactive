using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class TodoController : Controller
    {
        public IActionResult Index()
        {
            return View(new TodoModel());
        }

        [HttpPost]
        public IActionResult Save([FromBody] TodoModel? model)
        {
            if (model == null)
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Title"] = new[] { "Request body is required." } } });

            var validator = new TodoValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            return Ok(new { message = "Todo saved!" });
        }
    }
}
