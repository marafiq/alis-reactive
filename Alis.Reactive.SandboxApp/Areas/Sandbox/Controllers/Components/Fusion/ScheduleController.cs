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
        public IActionResult GetAssignments(string? selectedFacilityId, string? currentDate, string? currentView)
        {
            var facilityId = selectedFacilityId ?? "mystery-manor";
            DateTime anchor;
            if (!string.IsNullOrEmpty(currentDate) && DateTime.TryParse(currentDate, out var parsed))
                anchor = parsed;
            else
                anchor = DateTime.Today;

            var (rangeStart, rangeEnd) = CalculateDateRange(anchor, currentView ?? "Week");
            var data = FakeScheduleData.GetAssignments(facilityId, rangeStart, rangeEnd);
            return Ok(data);
        }

        [HttpGet("~/api/schedule/view/{currentView}/echo")]
        public IActionResult EchoView(string currentView)
        {
            return Ok(new ScheduleViewRouteResponse
            {
                CurrentView = currentView
            });
        }

        [HttpGet("~/api/schedule/view/{currentView}/summary")]
        public IActionResult ViewSummary(string currentView)
        {
            return Ok(new ScheduleViewRouteResponse
            {
                CurrentView = currentView,
                Summary = $"summary:{currentView}"
            });
        }

        [HttpPost("~/api/schedule/events/audit")]
        public IActionResult AuditEvents([FromBody] ScheduleEventsAuditRequest request)
        {
            return Ok(new ScheduleEventsAuditResponse
            {
                Count = request.Events.Count
            });
        }

        private static (DateTime start, DateTime end) CalculateDateRange(DateTime anchor, string view)
        {
            return view switch
            {
                "Day" => (anchor.Date, anchor.Date.AddDays(1)),
                "WorkWeek" => WorkWeekRange(anchor),
                "Month" => MonthRange(anchor),
                "Agenda" => (anchor.Date, anchor.Date.AddDays(30)),
                _ => WeekRange(anchor), // "Week" and default
            };
        }

        private static (DateTime start, DateTime end) WeekRange(DateTime anchor)
        {
            var sunday = anchor.Date.AddDays(-(int)anchor.DayOfWeek);
            return (sunday, sunday.AddDays(7));
        }

        private static (DateTime start, DateTime end) WorkWeekRange(DateTime anchor)
        {
            var monday = anchor.Date.AddDays(-(int)anchor.DayOfWeek + 1);
            if (anchor.DayOfWeek == DayOfWeek.Sunday) monday = monday.AddDays(-7);
            return (monday, monday.AddDays(5));
        }

        private static (DateTime start, DateTime end) MonthRange(DateTime anchor)
        {
            var firstOfMonth = new DateTime(anchor.Year, anchor.Month, 1);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            // SF Month view shows full weeks — pad to surrounding Sundays
            var rangeStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var rangeEnd = lastOfMonth.AddDays(6 - (int)lastOfMonth.DayOfWeek + 1);
            return (rangeStart, rangeEnd);
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
        /// Returns the new assignment form partial.
        /// Loaded into FusionDialog when user clicks an empty cell on the schedule.
        /// GroupIndex (0-based) is mapped to ShiftId (1-based).
        /// </summary>
        [HttpGet("NewAssignmentForm")]
        public IActionResult NewAssignmentForm(string startTime, string endTime, int shiftId, string? selectedFacilityId)
        {
            var model = new NewAssignmentModel
            {
                StartTime = startTime,
                EndTime = endTime,
                ShiftId = shiftId + 1, // GroupIndex 0-based → ShiftId 1-based
                FacilityId = selectedFacilityId ?? "mystery-manor",
            };
            return PartialView(
                "~/Areas/Sandbox/Views/Components/Fusion/Schedule/_NewAssignmentForm.cshtml",
                model);
        }

        /// <summary>
        /// Creates a new shift assignment. Called from the new assignment dialog form.
        /// </summary>
        [HttpPost("~/api/schedule/create-assignment")]
        public IActionResult CreateAssignment([FromBody] NewAssignmentModel? model)
        {
            if (model == null || model.StaffId == null)
                return BadRequest(new { errors = new { StaffId = new[] { "Staff member is required." } } });

            if (string.IsNullOrEmpty(model.StartTime) || string.IsNullOrEmpty(model.EndTime))
                return BadRequest(new { error = "Start and end time are required." });

            var assignment = FakeScheduleData.CreateAssignment(
                model.FacilityId ?? "mystery-manor",
                DateTime.Parse(model.StartTime),
                DateTime.Parse(model.EndTime),
                model.ShiftId,
                model.StaffId.Value);

            if (assignment == null)
                return BadRequest(new { error = "Could not create assignment. Staff not found." });

            return Ok(new
            {
                success = true,
                message = $"{assignment.StaffName} ({assignment.StaffRole}) assigned.",
                assignment,
            });
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
