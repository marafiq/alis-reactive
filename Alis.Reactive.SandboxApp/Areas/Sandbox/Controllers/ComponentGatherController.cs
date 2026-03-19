using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class ComponentGatherController : Controller
    {
        public IActionResult Index()
        {
            return View(new ComponentGatherModel
            {
                ResidentName = "Margaret Thompson",
                CareNotes = "Initial assessment completed.",
                ReceiveNotifications = true
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] ComponentGatherModel? model)
        {
            if (model == null)
                return BadRequest(new { error = "Empty body" });
            return Ok(model);
        }
    }
}
