using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// Staff training: the coordinator records a caregiver's training from the drawer. The course list,
    /// the calendar and the trainer list open over the form, and Escape closes only what is open.
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

        [HttpPost("StaffTraining/Record")]
        public IActionResult RecordStaffTraining([FromBody] StaffTrainingRecordModel? record)
        {
            if (record == null) return BadRequest();

            return Ok(new StaffTrainingRecorded($"Training recorded: {record.Notes}"));
        }
    }
}
