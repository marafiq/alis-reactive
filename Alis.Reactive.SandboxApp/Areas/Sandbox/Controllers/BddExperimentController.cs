using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class BddExperimentController : Controller
    {
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Submit([FromBody] object? body)
            => Ok(new BddExperimentResponse { Success = true, Message = "Resident admitted" });
    }
}
