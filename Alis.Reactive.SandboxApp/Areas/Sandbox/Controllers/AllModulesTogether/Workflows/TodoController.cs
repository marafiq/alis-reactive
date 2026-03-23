using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/Todo")]
    public class TodoController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/AllModulesTogether/Todo/Index.cshtml", new TodoModel());
        }

        [HttpPost("Save")]
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
