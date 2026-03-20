using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class DateValidationController : Controller
    {
        public IActionResult Index()
        {
            return View(new DateValidationModel());
        }

        [HttpPost]
        public IActionResult ValidateClient() => Ok(new { message = "Date validation passed!" });
    }
}
