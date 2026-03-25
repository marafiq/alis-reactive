using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/DateRules")]
    public class DateValidationController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Validation/DateRules/Index.cshtml", new DateValidationModel());
        }

        [HttpPost("ValidateClient")]
        public IActionResult ValidateClient() => Ok(new { message = "Date validation passed!" });
    }
}
