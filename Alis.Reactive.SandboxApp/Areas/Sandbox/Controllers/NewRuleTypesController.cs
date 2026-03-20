using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NewRuleTypesController : Controller
    {
        public IActionResult Index()
        {
            return View(new NewRuleTypesModel());
        }

        [HttpPost]
        public IActionResult ValidateClient() => Ok(new { message = "All new rule types passed!" });
    }
}
