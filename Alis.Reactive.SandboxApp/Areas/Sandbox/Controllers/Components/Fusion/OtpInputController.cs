using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/OtpInput")]
    public class OtpInputController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/OtpInput/Index.cshtml", new OtpInputModel
            {
                Passcode = "1234",
                AutoBlurCode = string.Empty
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] OtpInputEchoRequest request)
        {
            return Ok(new OtpInputEchoResponse
            {
                Passcode = request.Passcode,
                Summary = "code:" + request.Passcode
            });
        }
    }
}
