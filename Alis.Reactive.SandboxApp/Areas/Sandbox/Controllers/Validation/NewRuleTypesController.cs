using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/SpecializedRules")]
    public class NewRuleTypesController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Validation/SpecializedRules/Index.cshtml", new NewRuleTypesModel());
        }

        // The server runs the same validator on what the form sent: a value the browser accepts is one
        // the server accepts.
        [HttpPost("ValidateClient")]
        public IActionResult ValidateClient([FromBody] NewRuleTypesModel? model)
        {
            var result = new NewRuleTypesValidator().Validate(model ?? new NewRuleTypesModel());
            if (result.IsValid) return Ok(new { message = "All new rule types passed!" });

            return BadRequest(new { errors = result.ErrorsByField });
        }
    }
}
