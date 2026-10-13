using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Point-in-Time Schedule sandbox — server-driven shift schedule. Each browser's changes live in its
    /// own <see cref="ScheduleWorld"/>, so a save is visible on the next GET and to no other browser.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Schedule")]
    public class ScheduleController : Controller
    {
        private ScheduleWorld World => new(HttpContext.Session);

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Schedule/Index.cshtml",
                new PointInTimeScheduleModel { SelectedFacilityId = "mystery-manor" });
        }

        /// <summary>
        /// Loads the Schedule data source for the selected facility and visible range.
        /// Parameter names match the members of <see cref="ScheduleAssignmentsQuery"/> and
        /// <see cref="ScheduleNavigationQuery"/>, which the Reactive Plan gathers.
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
            var data = World.GetAssignments(facilityId, rangeStart, rangeEnd);
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
            // Syncfusion Month view shows full weeks — pad to surrounding Sundays
            var rangeStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
            var rangeEnd = lastOfMonth.AddDays(6 - (int)lastOfMonth.DayOfWeek + 1);
            return (rangeStart, rangeEnd);
        }

        /// <summary>
        /// Assigns staff to a shift slot in this browser's schedule.
        /// Subsequent data-source loads return the updated assignment.
        /// </summary>
        [HttpPost("~/api/schedule/assign")]
        public IActionResult AssignStaff([FromBody] AssignStaffRequest request)
        {
            if (request.StaffId == null)
                return BadRequest(new { error = "Staff member is required." });

            var updated = World.AssignStaff(request.AssignmentId, request.StaffId.Value);
            if (updated == null)
                return NotFound(new { error = "Assignment or staff not found." });

            return Ok(updated);
        }

        /// <summary>Takes a shift off this browser's schedule and returns it.</summary>
        [HttpPost("~/api/schedule/cancel-shift")]
        public IActionResult CancelShift([FromBody] CancelShiftRequest request)
        {
            var cancelled = World.CancelAssignment(request.AssignmentId);
            return cancelled == null ? NotFound(new { error = "Assignment not found." }) : Ok(cancelled);
        }

        /// <summary>
        /// Loads the event-edit partial into the NativeDrawer after a schedule event click.
        /// Partial has its own Reactive Plan that merges into the page's Active Plan.
        /// </summary>
        [HttpGet("EditForm")]
        public IActionResult EditForm(int assignmentId)
        {
            var assignment = World.FindAssignment(assignmentId);
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
        /// Loads the empty-cell assignment partial into the FusionDialog.
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
        /// Creates a shift assignment from the new-assignment dialog payload.
        /// </summary>
        [HttpPost("~/api/schedule/create-assignment")]
        public IActionResult CreateAssignment([FromBody] NewAssignmentModel? model)
        {
            if (model == null || model.StaffId == null)
                return BadRequest(new { errors = new { StaffId = new[] { "Staff member is required." } } });

            if (string.IsNullOrEmpty(model.StartTime) || string.IsNullOrEmpty(model.EndTime))
                return BadRequest(new { error = "Start and end time are required." });

            var assignment = World.CreateAssignment(
                model.FacilityId ?? "mystery-manor",
                DateTime.Parse(model.StartTime),
                DateTime.Parse(model.EndTime),
                model.ShiftId,
                model.StaffId.Value);

            if (assignment == null)
                return BadRequest(new { error = "Could not create assignment. Staff not found." });

            return Ok(assignment);
        }

        /// <summary>
        /// Loads staff options for assignment dialog dropdowns.
        /// </summary>
        [HttpGet("~/api/schedule/staff")]
        public IActionResult GetStaff()
        {
            return Ok(FakeScheduleData.Staff);
        }

        /// <summary>
        /// Loads facility options for the Schedule filter dropdown.
        /// </summary>
        [HttpGet("~/api/schedule/facilities")]
        public IActionResult GetFacilities()
        {
            return Ok(FakeScheduleData.Facilities);
        }
    }
}
