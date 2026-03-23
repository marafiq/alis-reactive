using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/SpecializedRules")]
    public class NewRuleTypesController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Validation/SpecializedRules/Index.cshtml", new NewRuleTypesModel());
        }

        [HttpPost("ValidateClient")]
        public IActionResult ValidateClient() => Ok(new { message = "All new rule types passed!" });
    }
}
