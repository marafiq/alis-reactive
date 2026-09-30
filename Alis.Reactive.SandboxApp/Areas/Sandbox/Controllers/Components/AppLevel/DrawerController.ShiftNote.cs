using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Shift notes: a resident opens in the drawer, where the nurse writes a shift note or
    /// starts a discharge that must be confirmed first.
    /// </summary>
    public partial class DrawerController
    {
        private const int MargaretHillId = 2107;

        [HttpGet("ShiftNote")]
        public IActionResult ShiftNote() =>
            View("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/ShiftNote.cshtml", new ShiftNotePageModel());

        [HttpGet("ShiftNote/Form")]
        public IActionResult ShiftNoteForm() =>
            PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_ShiftNotePartial.cshtml",
                new ShiftNoteModel { ResidentId = MargaretHillId });

        [HttpPost("ShiftNote/Save")]
        public IActionResult SaveShiftNote([FromBody] ShiftNoteModel? note)
        {
            if (note == null || note.ResidentId != MargaretHillId || string.IsNullOrWhiteSpace(note.Note))
                return BadRequest();

            return Ok(new ShiftNoteSaved("Shift note saved for Margaret Hill."));
        }

        [HttpPost("ShiftNote/Discharge")]
        public IActionResult DischargeResident([FromBody] ShiftNoteModel? note)
        {
            if (note == null || note.ResidentId != MargaretHillId) return BadRequest();

            return Ok(new ResidentDischarged("Margaret Hill has been discharged."));
        }
    }
}
