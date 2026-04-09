using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Point-in-Time Schedule sandbox — server-driven shift schedule.
    /// Mutations persist in-memory so POST changes are visible on GET refresh.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Schedule")]
    public class ScheduleController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Schedule/Index.cshtml",
                new PointInTimeScheduleModel { SelectedFacilityId = "mystery-manor" });
        }

        /// <summary>
        /// Returns shift assignments for a facility and week.
        /// Parameter names match the model property and event arg names used by
        /// Include and FromEvent gather in the reactive plan.
        /// </summary>
        [HttpGet("~/api/schedule/assignments")]
        public IActionResult GetAssignments(string? selectedFacilityId, string? currentDate)
        {
            var facilityId = selectedFacilityId ?? "mystery-manor";
            DateTime weekStart;
            if (!string.IsNullOrEmpty(currentDate) && DateTime.TryParse(currentDate, out var parsed))
                weekStart = parsed;
            else
                weekStart = DateTime.Today;

            var data = FakeScheduleData.GetAssignments(facilityId, weekStart);
            return Ok(data);
        }

        /// <summary>
        /// Assigns a staff member to a shift slot. Mutates in-memory store.
        /// After this POST, a subsequent GET returns the updated assignment.
        /// </summary>
        [HttpPost("~/api/schedule/assign")]
        public IActionResult AssignStaff([FromBody] AssignStaffRequest request)
        {
            if (request.StaffId == null)
                return BadRequest(new { error = "Staff member is required." });

            var updated = FakeScheduleData.AssignStaff(request.AssignmentId, request.StaffId.Value);
            if (updated == null)
                return NotFound(new { error = "Assignment or staff not found." });

            return Ok(new
            {
                success = true,
                message = $"{updated.StaffName} ({updated.StaffRole}) assigned.",
                assignment = updated,
            });
        }

        /// <summary>
        /// Removes staff from a shift slot (makes it unassigned). Mutates in-memory store.
        /// </summary>
        [HttpPost("~/api/schedule/unassign")]
        public IActionResult UnassignStaff([FromBody] AssignStaffRequest request)
        {
            var updated = FakeScheduleData.UnassignStaff(request.AssignmentId);
            if (updated == null)
                return NotFound(new { error = "Assignment not found." });

            return Ok(new
            {
                success = true,
                message = $"Slot unassigned. Needs coverage.",
                assignment = updated,
            });
        }

        /// <summary>
        /// Returns the edit assignment form partial.
        /// Loaded into the NativeDrawer when user clicks an event on the schedule.
        /// The partial has its own ReactivePlan that merges into the page.
        /// </summary>
        [HttpGet("EditForm")]
        public IActionResult EditForm(int assignmentId)
        {
            // Look up current assignment to pre-populate the form
            var assignment = FakeScheduleData.FindAssignment(assignmentId);
            var currentStaffId = assignment?.StaffName != null
                ? FakeScheduleData.Staff.FirstOrDefault(s => s.Name == assignment.StaffName)?.Id
                : null;

            var model = new EditAssignmentModel
            {
                AssignmentId = assignmentId,
                StaffId = currentStaffId,
                Notes = assignment?.Description,
            };
            return PartialView(
                "~/Areas/Sandbox/Views/Components/Fusion/Schedule/_EditAssignmentForm.cshtml",
                model);
        }

        /// <summary>
        /// Returns available staff for the assignment dialog dropdown.
        /// </summary>
        [HttpGet("~/api/schedule/staff")]
        public IActionResult GetStaff()
        {
            return Ok(FakeScheduleData.Staff);
        }

        /// <summary>
        /// Returns facility list for the facility filter dropdown.
        /// </summary>
        [HttpGet("~/api/schedule/facilities")]
        public IActionResult GetFacilities()
        {
            return Ok(FakeScheduleData.Facilities);
        }
    }
}
