using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.TextArea;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/TextArea")]
    public class TextAreaController : Controller
    {
        private const string NightShiftNote =
            "Slept through until 5:40, then asked for tea. Took her evening medications with applesauce.";

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/TextArea/Index.cshtml", new DailyCareLogModel
            {
                NightShiftNote = NightShiftNote
            });
        }

        // The log keeps a draft each time the caregiver leaves the note.
        [HttpPost("KeepDraft")]
        public IActionResult KeepDraft([FromBody] CareLogDraft draft)
        {
            var length = draft.Note?.Length ?? 0;
            return Ok(new CareLogAnswer
            {
                Message = length == 0 ? "Nothing to keep yet." : $"Draft kept: {Characters(length)}."
            });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] CareLogEntry entry)
        {
            var result = new DailyCareLogValidator().Validate(new DailyCareLogModel
            {
                CareNote = entry.CareNote,
                Signed = entry.Signed
            });
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new CareLogAnswer
            {
                Message = $"Saved Margaret Hale's day-shift note to the daily log: {Characters(entry.CareNote!.Length)}."
            });
        }

        private static string Characters(int length) => length == 1 ? "1 character" : $"{length} characters";
    }
}
