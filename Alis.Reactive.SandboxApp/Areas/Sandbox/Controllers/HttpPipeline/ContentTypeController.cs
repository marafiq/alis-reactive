using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ContentType")]
    public class ContentTypeController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/HttpPipeline/ContentType/Index.cshtml");
        }

        /// <summary>Returns a flat JSON response.</summary>
        [HttpGet("FlatJson")]
        public IActionResult FlatJson()
        {
            return Json(new { message = "Hello from server", count = 42 });
        }

        /// <summary>Returns a nested JSON response with multiple levels.</summary>
        [HttpGet("NestedJson")]
        public IActionResult NestedJson()
        {
            return Json(new
            {
                data = new
                {
                    user = new { name = "Jane Doe", email = "jane@example.com" },
                    total = 99.5m
                }
            });
        }

        /// <summary>Returns HTML partial with native + TestWidget components.</summary>
        [HttpGet("Partial")]
        public IActionResult Partial()
        {
            return PartialView("~/Areas/Sandbox/Views/HttpPipeline/ContentType/_ContentTypePartial.cshtml", new ContentTypeModel
            {
                NativeValue = "native-partial-value",
                FusionAmount = 42
            });
        }
    }
}
