using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class SwitchController : Controller
    {
        public IActionResult Index()
        {
            return View(new SwitchModel
            {
                ReceiveNotifications = true,
                EmailAlerts = false,
                SmsAlerts = false
            });
        }
    }
}
