using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ContentTypeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>Returns a flat JSON response.</summary>
        [HttpGet]
        public IActionResult FlatJson()
        {
            return Json(new { message = "Hello from server", count = 42 });
        }

        /// <summary>Returns a nested JSON response with multiple levels.</summary>
        [HttpGet]
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
        [HttpGet]
        public IActionResult Partial()
        {
            return PartialView("_ContentTypePartial", new Models.ContentTypeModel
            {
                NativeValue = "native-partial-value",
                FusionAmount = 42
            });
        }
    }
}
