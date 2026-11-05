using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Staff training: the coordinator records a caregiver's training in a drawer that stays open on
    /// Escape, and reads past training in a drawer that Escape closes.
    /// </summary>
    public partial class DrawerController
    {
        [HttpGet("StaffTraining")]
        public IActionResult StaffTraining() =>
            View("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/StaffTraining.cshtml", new StaffTrainingPageModel());

        [HttpGet("StaffTraining/Form")]
        public IActionResult StaffTrainingForm() =>
            PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_StaffTrainingPartial.cshtml",
                new StaffTrainingRecordModel());

        [HttpGet("StaffTraining/History")]
        public IActionResult StaffTrainingHistory() =>
            PartialView("~/Areas/Sandbox/Views/Components/AppLevel/Drawer/_StaffTrainingHistoryPartial.cshtml",
                StaffTrainingOptions.History);

        [HttpPost("StaffTraining/Record")]
        public IActionResult RecordStaffTraining([FromBody] StaffTrainingRecordModel? record)
        {
            if (record == null) return BadRequest();

            return Ok(new StaffTrainingRecorded($"Training recorded: {record.Notes}"));
        }
    }
}
