using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Switch")]
    public class SwitchController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Switch/Index.cshtml", new SwitchModel
            {
                ReceiveCareAlerts = true,
                EmailReminders = true,
                TextMessageAlerts = false
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] CareAlertPreferencesRequest request)
        {
            return Ok(new CareAlertPreferencesResponse
            {
                Summary = Summarize(request)
            });
        }

        private static string Summarize(CareAlertPreferencesRequest request)
        {
            if (!request.ReceiveCareAlerts)
            {
                return "Saved. Care alerts are paused. We will not send reminders until you turn them back on.";
            }

            var channels = new System.Collections.Generic.List<string>();
            if (request.EmailReminders)
            {
                channels.Add("email");
            }

            if (request.TextMessageAlerts)
            {
                channels.Add("text message");
            }

            if (channels.Count == 0)
            {
                return "Saved. Care alerts are on, but no delivery channel is selected. Please choose email or text message.";
            }

            return "Saved. We will send your care alerts by " + string.Join(" and ", channels) + ".";
        }
    }
}
