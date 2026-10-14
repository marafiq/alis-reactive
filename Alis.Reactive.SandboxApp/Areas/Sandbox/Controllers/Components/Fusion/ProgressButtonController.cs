using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ProgressButton;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionProgressButton")]
    public sealed class ProgressButtonController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/ProgressButton/Index.cshtml", new FusionProgressButtonModel());

        // Riverside's system confirms the moment it has the report; Lakeside confirms once the upload
        // finishes; County takes the pass by fax.
        [HttpPost("Send")]
        public IActionResult Send([FromBody] MedicationPassSendRequest request) => request.Pharmacy switch
        {
            "Lakeside" => Ok(new MedicationPassSendReply { Reply = "Lakeside Pharmacy confirms once the upload finishes." }),
            "Riverside" => Ok(new MedicationPassSendReply { ConfirmedOnReceipt = true, Reply = "Riverside Pharmacy confirmed it has the report." }),
            "County" => Ok(new MedicationPassSendReply { Reply = "County Pharmacy confirms faxes by phone." }),
            _ => BadRequest()
        };
    }
}
