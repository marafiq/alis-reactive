using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The admissions desk: the admissions coordinator corrects a resident's care level, primary nurse and next
    /// care review in the residents' records, and admits the next referral waiting to move in by completing its
    /// admission form. Every page starts from the records on file; a correction or an admission answers with
    /// what it did and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        // A first care review falls within this many days of moving in.
        private const int AdmissionsFirstReviewWithinDays = 14;

        [HttpGet("ResidentEditForm"), ActionName("ResidentEditForm")]
        public IActionResult AdmissionsDeskPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ResidentEditForm.cshtml",
                new AdmissionsDeskViewModel
                {
                    CareLevels = new[] { "Independent", "Assisted Living", "Memory Care", "Skilled Nursing" },
                    RiskLevels = new[] { "Low", "Moderate", "High" },
                    Nurses = new[]
                    {
                        new AdmissionsNurse("Nora Ellis", "days"),
                        new AdmissionsNurse("Malik Stone", "days"),
                        new AdmissionsNurse("Elena Ruiz", "evenings"),
                        new AdmissionsNurse("Owen Park", "nights")
                    }
                });

        [HttpGet("ResidentEditFormDesk")]
        public IActionResult ResidentEditFormDesk() =>
            Ok(new AdmissionsDesk { Residents = AdmissionsResidentsOnFile(), Referrals = AdmissionsReferralsOnFile() });

        [HttpPost("ResidentEditFormCorrect")]
        public IActionResult ResidentEditFormCorrect([FromBody] ResidentRecordCorrection correction)
        {
            var resident = correction.Resident;
            return Ok(new AdmissionsAnswer
            {
                Summary = $"{resident.ResidentName}'s record now reads {resident.CareLevel} with {resident.PrimaryNurse}, next care review {AdmissionsDay(resident.NextReview)}."
            });
        }

        // A first care review falls between the day the resident moves in and two weeks after; a referral
        // whose form says otherwise stays waiting, as it is on file.
        [HttpPost("ResidentEditFormAdmit")]
        public IActionResult ResidentEditFormAdmit([FromBody] ReferralAdmission admission)
        {
            var completed = admission.Referral;
            var onFile = AdmissionsReferralsOnFile().FirstOrDefault(r => r.ReferralId == completed.ReferralId);
            if (onFile == null)
                return NotFound();

            var moveIn = AdmissionsDate(onFile.MoveInDay);
            var latest = moveIn.AddDays(AdmissionsFirstReviewWithinDays);
            var reviewIsOnTime = DateOnly.TryParseExact(completed.FirstReview, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var review)
                && review >= moveIn && review <= latest;
            if (!reviewIsOnTime)
                return BadRequest(new AdmissionsDesk
                {
                    Residents = AdmissionsResidentsOnFile(),
                    Referrals = AdmissionsReferralsOnFile(),
                    Summary = $"{completed.ResidentName}'s first care review must fall between moving in on {AdmissionsDay(onFile.MoveInDay)} and {latest.ToString("dddd, MMMM d", CultureInfo.InvariantCulture)}."
                });

            var residents = AdmissionsResidentsOnFile();
            residents.Add(new AdmissionsResident
            {
                ResidentId = residents.Max(r => r.ResidentId) + 1,
                ResidentName = completed.ResidentName,
                Suite = onFile.Suite,
                CareLevel = onFile.CareLevel,
                PrimaryNurse = onFile.PrimaryNurse,
                NextReview = completed.FirstReview
            });
            var tasks = completed.OpenTasks == 1 ? "1 open care task" : $"{completed.OpenTasks} open care tasks";
            return Ok(new AdmissionsDesk
            {
                Residents = residents.OrderBy(r => r.ResidentName, StringComparer.Ordinal).ToList(),
                Referrals = AdmissionsReferralsOnFile().Where(r => r.ReferralId != completed.ReferralId).ToList(),
                Summary = $"Welcome, {completed.ResidentName}: admitted at {completed.RiskLevel} risk with {tasks}, first care review {AdmissionsDay(completed.FirstReview)}."
            });
        }

        private static DateOnly AdmissionsDate(string day) => DateOnly.ParseExact(day, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // "Tuesday, October 20", as the coordinator reads a day; a record left without one says so.
        private static string AdmissionsDay(string day) =>
            DateOnly.TryParseExact(day, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.ToString("dddd, MMMM d", CultureInfo.InvariantCulture)
                : "not booked";

        private static List<AdmissionsResident> AdmissionsResidentsOnFile() => new()
        {
            new AdmissionsResident { ResidentId = 6501, ResidentName = "Amina Patel", Suite = "201", CareLevel = "Assisted Living", PrimaryNurse = "Malik Stone", NextReview = "2026-10-22" },
            new AdmissionsResident { ResidentId = 6502, ResidentName = "Grace Bennett", Suite = "207", CareLevel = "Independent", PrimaryNurse = "Owen Park", NextReview = "2026-11-12" },
            new AdmissionsResident { ResidentId = 6503, ResidentName = "Henry Liu", Suite = "204", CareLevel = "Assisted Living", PrimaryNurse = "Elena Ruiz", NextReview = "2026-10-16" },
            new AdmissionsResident { ResidentId = 6504, ResidentName = "Ruth Carter", Suite = "212", CareLevel = "Memory Care", PrimaryNurse = "Malik Stone", NextReview = "2026-10-28" },
            new AdmissionsResident { ResidentId = 6505, ResidentName = "Walter Grant", Suite = "210", CareLevel = "Skilled Nursing", PrimaryNurse = "Nora Ellis", NextReview = "2026-12-03" }
        };

        private static List<AdmissionsReferral> AdmissionsReferralsOnFile() => new()
        {
            new AdmissionsReferral { ReferralId = 8101, ResidentName = "Rosa Diaz", MoveInDay = "2026-10-19", Suite = "214", CareLevel = "Memory Care", PrimaryNurse = "Elena Ruiz", RiskLevel = "Moderate", OpenTasks = 2, FirstReview = "2026-10-26" },
            new AdmissionsReferral { ReferralId = 8102, ResidentName = "Oscar Flynn", MoveInDay = "2026-10-21", Suite = "216", CareLevel = "Assisted Living", PrimaryNurse = "Nora Ellis", RiskLevel = "Low", OpenTasks = 1, FirstReview = "2026-10-28" },
            new AdmissionsReferral { ReferralId = 8103, ResidentName = "Vera Lindqvist", MoveInDay = "2026-10-26", Suite = "218", CareLevel = "Skilled Nursing", PrimaryNurse = "Owen Park", RiskLevel = "High", OpenTasks = 4, FirstReview = "2026-11-02" }
        };
    }
}
