using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiColumnComboBox;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Maple Grove's nursing coverage for the week of October 19: the open shifts, how each stands with the
    /// staffing line, the nurses the coordinator can call in with the hours they have worked, and the
    /// assignment itself.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/MultiColumnComboBox")]
    public sealed class MultiColumnComboBoxController : Controller
    {
        private const string Staff = "Maple Grove staff";
        private const string Agency = "CareBridge agency";
        private const int WeeklyHours = 40;

        // Willow is the memory care wing: a nurse who covers it is certified in dementia care.
        private static readonly IReadOnlyList<CoverageShift> Shifts =
        [
            new("willow-night", "Tonight 19:00–07:00, Willow (Priya Raman called out)", "the Willow night shift tonight, 19:00–07:00",
                NeedsDementiaCare: true, Volunteer: "", CoveredBy: ""),
            new("maple-day", "Tomorrow 07:00–15:00, Maple (Lena Ortiz on leave)", "the Maple day shift tomorrow, 07:00–15:00",
                NeedsDementiaCare: false, Volunteer: "N-204", CoveredBy: ""),
            new("cedar-evening", "Saturday 15:00–23:00, Cedar (Omar Haddad in training)", "the Cedar evening shift on Saturday, 15:00–23:00",
                NeedsDementiaCare: false, Volunteer: "", CoveredBy: "Ana Lima")
        ];

        private static readonly IReadOnlyList<RosterNurse> Nurses =
        [
            new() { Id = "N-201", Name = "Sam Ortiz", HomeWing = "Maple", Shift = "Days", Certifications = "LPN", Employer = Staff },
            new() { Id = "N-202", Name = "Grace Kim", HomeWing = "Willow", Shift = "Nights", Certifications = "RN, Dementia care", Employer = Staff },
            new() { Id = "N-203", Name = "Ana Lima", HomeWing = "Cedar", Shift = "Evenings", Certifications = "RN", Employer = Staff },
            new() { Id = "N-204", Name = "Jordan Ellis", HomeWing = "Willow", Shift = "Evenings", Certifications = "LPN, Dementia care", Employer = Staff },
            new() { Id = "N-205", Name = "Marcus Bell", HomeWing = "Maple", Shift = "Nights", Certifications = "RN", Employer = Staff },
            new() { Id = "A-301", Name = "Kim Tran", HomeWing = "Any", Shift = "Nights", Certifications = "RN, Dementia care", Employer = Agency },
            new() { Id = "A-302", Name = "Leah Novak", HomeWing = "Any", Shift = "Days", Certifications = "LPN", Employer = Agency }
        ];

        private static readonly IReadOnlyDictionary<string, int> HoursThisWeek = new Dictionary<string, int>
        {
            ["N-201"] = 32, ["N-202"] = 24, ["N-203"] = 36, ["N-204"] = 16, ["N-205"] = 40, ["A-301"] = 12, ["A-302"] = 0
        };

        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/MultiColumnComboBox/Index.cshtml", new FusionMultiColumnComboBoxModel
            {
                OpenShifts = Shifts.Select(shift => new OpenShiftOption { Code = shift.Code, Label = shift.Label }).ToList(),
                Roster = RosterOf(includeAgency: false)
            });

        [HttpGet("ShiftStatus")]
        public IActionResult ShiftStatus([FromQuery] ShiftStatusQuery query)
        {
            var shift = FindShift(query.Shift);
            return shift is null
                ? NotFound()
                : Ok(new ShiftStanding { Volunteer = shift.Volunteer, CoveredBy = shift.CoveredBy });
        }

        [HttpGet("Roster")]
        public IActionResult Roster([FromQuery] RosterQuery query) =>
            Ok(new CoverageRoster { Nurses = RosterOf(query.IncludeAgency) });

        [HttpGet("Hours")]
        public IActionResult Hours([FromQuery] NurseHoursQuery query)
        {
            var nurse = FindNurse(query.Nurse);
            if (nurse is null)
                return NotFound();

            var hours = HoursThisWeek[nurse.Id];
            return Ok(new NurseWeek
            {
                Summary = hours >= WeeklyHours
                    ? $"{nurse.Name} has worked {hours} of {WeeklyHours} hours this week; this shift would be overtime."
                    : $"{nurse.Name} has worked {hours} of {WeeklyHours} hours this week."
            });
        }

        [HttpPost("Assign")]
        public IActionResult Assign([FromBody] FusionMultiColumnComboBoxModel coverage)
        {
            var shift = FindShift(coverage.OpenShift);
            var nurse = FindNurse(coverage.CoveringNurse);
            if (shift is null || nurse is null)
                return BadRequest(new CoverageConfirmation { Message = "Choose the open shift and the nurse who covers it." });
            if (shift.CoveredBy.Length > 0)
                return BadRequest(new CoverageConfirmation { Message = $"{shift.CoveredBy} already covers {shift.Description}." });
            if (shift.NeedsDementiaCare && !nurse.Certifications.Contains("Dementia care"))
                return BadRequest(new CoverageConfirmation { Message = $"{nurse.Name} is not certified in dementia care; Willow needs a nurse who is." });

            return Ok(new CoverageConfirmation { Message = $"{nurse.Name} covers {shift.Description}." });
        }

        private static IReadOnlyList<RosterNurse> RosterOf(bool includeAgency) =>
            Nurses.Where(nurse => includeAgency || nurse.Employer == Staff).ToList();

        private static CoverageShift? FindShift(string? code) => Shifts.FirstOrDefault(shift => shift.Code == code);

        private static RosterNurse? FindNurse(string? id) => Nurses.FirstOrDefault(nurse => nurse.Id == id);

        private sealed record CoverageShift(string Code, string Label, string Description, bool NeedsDementiaCare, string Volunteer, string CoveredBy);
    }
}
