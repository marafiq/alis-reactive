using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.ResponseConditions;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions
{
    [Area("Sandbox")]
    [Route("Sandbox/Conditions/ResponseConditions")]
    public class ResponseConditionsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Conditions/ResponseConditions/Index.cshtml",
                new ResponseConditionsModel());
        }

        [HttpPost("Approve")]
        public IActionResult Approve()
        {
            return Ok(new { status = "approved", count = 5 });
        }

        [HttpPost("Deny")]
        public IActionResult Deny()
        {
            return Ok(new { status = "denied", count = 0 });
        }

        [HttpPost("Pending")]
        public IActionResult Pending()
        {
            return Ok(new { status = "pending", count = 3 });
        }

        [HttpPost("Fail")]
        public IActionResult Fail()
        {
            return StatusCode(422, new { message = "Validation failed", code = 422 });
        }
    }
}
