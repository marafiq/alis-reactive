using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The staffing coordinator's shift schedule. Each browser's changes live in its own
    /// <see cref="ScheduleWorld"/>, so a save is visible on the next GET and to no other browser.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/Schedule")]
    public class ScheduleController : Controller
    {
        private const string DefaultFacilityId = "maple-grove";

        private ScheduleWorld World => new(HttpContext.Session);

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Schedule/Index.cshtml",
                new PointInTimeScheduleModel { SelectedFacilityId = DefaultFacilityId });
        }

        /// <summary>
        /// The facility's shifts for the dates a view shows around a date, and the server's summary of them.
        /// The view is in the path (<c>{currentView}</c>), as the page gathers it from the schedule or from
        /// its navigating event; the date and the facility are in the query.
        /// </summary>
        [HttpGet("Shifts/{currentView}")]
        public IActionResult Shifts(string currentView, string? selectedFacilityId, DateTime? currentDate)
        {
            var facility = FakeScheduleData.Facilities.FirstOrDefault(f => f.Id == selectedFacilityId);
            if (facility == null || currentDate == null)
                return BadRequest(new { error = "Say which facility and which date the schedule shows." });

            var (rangeStart, rangeEnd) = CalculateDateRange(currentDate.Value, currentView);
            return Ok(World.GetAssignments(facility, rangeStart, rangeEnd));
        }

        /// <summary>
        /// Asks the staffing agency to cover the open shifts among those the schedule holds.
        /// </summary>
        [HttpPost("AgencyRequest")]
        public IActionResult AgencyRequest([FromBody] AgencyCoverRequest request)
        {
            var open = request.Shifts.Count(s => s.IsUnassigned && !s.IsInService);
            var summary = open == 0
                ? "Every shift on the schedule is covered; nothing was sent to Staffing Plus."
                : string.Format(CultureInfo.InvariantCulture, "Asked Staffing Plus to cover {0} open {1}.", open, open == 1 ? "shift" : "shifts");
            return Ok(new AgencyCoverAnswer { Summary = summary });
        }

        /// <summary>
        /// Answers whether the in-service opened in the schedule's editor is the coordinator's to change:
        /// 409 while someone else has it open.
        /// </summary>
        [HttpPost("InServiceClaim")]
        public IActionResult InServiceClaim([FromBody] InServiceClaimRequest request)
        {
            var claim = World.ClaimInService(request.InServiceId);
            if (claim == null) return NotFound(new { error = "In-service not found." });

            var (refused, answer) = claim.Value;
            return refused ? Conflict(answer) : Ok(answer);
        }

        /// <summary>
        /// Saves an in-service changed in the schedule's editor, which the page holds back from the schedule until
        /// the server answers what it saved.
        /// </summary>
        [HttpPost("SaveInService")]
        public IActionResult SaveInService([FromBody] InServiceChange change)
        {
            var answer = World.SaveInService(change.InServiceId, change.Subject, change.Description);
            return answer == null ? NotFound(new { error = "In-service not found." }) : Ok(answer);
        }

        /// <summary>Takes an in-service deleted in the schedule's editor off this browser's schedule.</summary>
        [HttpPost("RemoveInService")]
        public IActionResult RemoveInService([FromBody] InServiceRemoval removal)
        {
            var answer = World.RemoveInService(removal.InServiceId);
            return answer == null ? NotFound(new { error = "In-service not found." }) : Ok(answer);
        }

        private static (DateTime start, DateTime end) CalculateDateRange(DateTime anchor, string view)
        {
            return view switch
            {
                "Day" => (anchor.Date, anchor.Date.AddDays(1)),
                "WorkWeek" => WorkWeekRange(anchor),
                "Month" => MonthRange(anchor),
                "Agenda" => (anchor.Date, anchor.Date.AddDays(7)),
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
        /// Loads the empty-cell assignment partial into the FusionDialog. The cell's shift column (0-based)
        /// is its shift (1-based). The query names the column for what it is: a query value bound under the
        /// model's own name (<c>ShiftId</c>) is what MVC's hidden field sends, not the model's value.
        /// </summary>
        [HttpGet("NewAssignmentForm")]
        public IActionResult NewAssignmentForm([FromQuery] NewAssignmentQuery query)
        {
            var model = new NewAssignmentModel
            {
                StartTime = query.StartTime.ToString("s", CultureInfo.InvariantCulture),
                EndTime = query.EndTime.ToString("s", CultureInfo.InvariantCulture),
                ShiftId = query.GroupIndex + 1,
                FacilityId = query.SelectedFacilityId ?? DefaultFacilityId,
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
                model.FacilityId ?? DefaultFacilityId,
                DateTime.Parse(model.StartTime),
                DateTime.Parse(model.EndTime),
                model.ShiftId,
                model.StaffId.Value);

            if (assignment == null)
                return BadRequest(new { error = "Could not create assignment. Staff not found." });

            return Ok(assignment);
        }
    }
}
