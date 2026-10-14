using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.OtpInput;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/OtpInput")]
    public class OtpInputController : Controller
    {
        // Today's one booked visit, and the code the desk gives a visitor who did not book.
        private const string BookedCode = "4821";
        private const string WalkInCode = "7305";

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/OtpInput/Index.cshtml", new OtpInputModel());
        }

        [HttpPost("Check")]
        public IActionResult Check([FromBody] VisitCodeCheck check)
        {
            return Ok(new VisitCodeChecked { Message = MessageFor(check) });
        }

        [HttpPost("WalkIn")]
        public IActionResult WalkIn()
        {
            return Ok(new WalkInCodeIssued { Code = WalkInCode });
        }

        private static string MessageFor(VisitCodeCheck check)
        {
            if (check.Code == BookedCode)
                return "Daniel O'Brien is checked in to visit Margaret Hale in Maple 214.";

            var previousCodeMissedToo = !string.IsNullOrEmpty(check.PreviousCode) && check.PreviousCode != BookedCode;
            return previousCodeMissedToo
                ? $"No visit today matches {check.Code} either. Ask the visitor to read the text message again."
                : $"No visit today matches {check.Code}.";
        }
    }
}
