using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TextArea")]
    public class TextAreaController : Controller
    {
        private const string LastShiftNote =
            "Resident ate a full breakfast and walked the garden loop with assistance.";

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/TextArea/Index.cshtml", new TextAreaModel
            {
                CareNote = LastShiftNote
            });
        }

        [HttpPost("Echo")]
        public IActionResult Echo([FromBody] CareNoteEchoRequest request)
        {
            return Ok(new CareNoteEchoResponse
            {
                CareNote = request.CareNote,
                Summary = "Saved to the resident's daily log: “" + request.CareNote + "”"
            });
        }
    }
}
