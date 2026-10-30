using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The East wing's care reviews: the director of nursing goes through the wing's twelve residents six to a
    /// page, puts the residents she picks on Thursday's care conference agenda, and starts a resident's review
    /// or asks housekeeping to ready their suite from their row. Every page starts from the same list, and an answer works from what the page sent,
    /// keeping nothing.
    /// </summary>
    public partial class GridController
    {
        private const string WingReviewUnderWay = "Under way";

        [HttpGet("CareReview"), ActionName("CareReview")]
        public IActionResult WingReviewPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareReview.cshtml",
                new WingReviewViewModel { Wing = "East", Director = "Joan Avery, RN", PageSize = 6 });

        // A waiting resident's bed is missing until one is free, and the page's row button fills its payload from
        // it: the view is refused as it renders.
        [HttpGet("CareReviewWaitingList")]
        public IActionResult WingReviewWaitingList() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareReviewWaitingList.cshtml",
                new WingReviewViewModel { Wing = "East", Director = "Joan Avery, RN", PageSize = 6 });

        [HttpGet("WingReviewResidents")]
        public IActionResult WingReviewResidents() => Ok(WingReviewsOnFile());

        [HttpPost("WingReviewStart")]
        public IActionResult WingReviewStart([FromBody] WingReviewStart start)
        {
            var resident = WingReviewsOnFile().FirstOrDefault(r => r.ResidentId == start.ResidentId);
            if (resident == null)
                return BadRequest(new WingReviewRefusal { Summary = "That resident is not on the East wing." });
            if (resident.Review == WingReviewUnderWay)
                return BadRequest(new WingReviewRefusal { Summary = $"{resident.ResidentName}'s review is already under way." });

            return Ok(new WingReviewStarted
            {
                ResidentId = resident.ResidentId,
                Review = WingReviewUnderWay,
                Summary = $"Review started for {resident.ResidentName}, suite {resident.Suite}."
            });
        }

        [HttpPost("WingReviewReadySuite")]
        public IActionResult WingReviewReadySuite([FromBody] WingReviewSuiteVisit visit)
        {
            var resident = WingReviewsOnFile().FirstOrDefault(r => r.Suite == visit.Suite);
            if (resident == null)
                return BadRequest(new WingReviewRefusal { Summary = "That suite is not on the East wing." });

            return Ok(new WingReviewSuiteReady
            {
                Summary = $"Housekeeping will ready suite {resident.Suite} before {resident.ResidentName}'s review."
            });
        }

        [HttpPost("WingReviewConference")]
        public IActionResult WingReviewConference([FromBody] WingReviewConference conference)
        {
            var names = conference.Residents.Select(r => r.ResidentName).ToList();
            return Ok(new WingReviewAgenda
            {
                Summary = names.Count switch
                {
                    0 => "Nobody was added to the care conference agenda.",
                    1 => $"{names[0]} is on Thursday's care conference agenda.",
                    _ => $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]} are on Thursday's care conference agenda."
                }
            });
        }

        // Page one carries one high-risk resident (Katherine Ortiz), page two three (Noah Walsh, Ruth Carter and
        // Samuel Diaz). Henry Liu's review started last week.
        private static List<WingReviewResident> WingReviewsOnFile() => new()
        {
            new() { ResidentId = 7101, ResidentName = "Amina Patel", Suite = "E-101", RiskLevel = "Low", ReviewDue = "June 2", Review = "Not started" },
            new() { ResidentId = 7102, ResidentName = "Grace Bennett", Suite = "E-104", RiskLevel = "Moderate", ReviewDue = "June 3", Review = "Not started" },
            new() { ResidentId = 7103, ResidentName = "Henry Liu", Suite = "E-107", RiskLevel = "Moderate", ReviewDue = "June 3", Review = WingReviewUnderWay },
            new() { ResidentId = 7104, ResidentName = "Irene Morgan", Suite = "E-110", RiskLevel = "Low", ReviewDue = "June 5", Review = "Not started" },
            new() { ResidentId = 7105, ResidentName = "Jonah Reed", Suite = "E-112", RiskLevel = "Low", ReviewDue = "June 9", Review = "Not started" },
            new() { ResidentId = 7106, ResidentName = "Katherine Ortiz", Suite = "E-115", RiskLevel = "High", ReviewDue = "June 10", Review = "Not started" },
            new() { ResidentId = 7107, ResidentName = "Leo Simmons", Suite = "E-118", RiskLevel = "Moderate", ReviewDue = "June 11", Review = "Not started" },
            new() { ResidentId = 7108, ResidentName = "Mara Thompson", Suite = "E-201", RiskLevel = "Low", ReviewDue = "June 12", Review = "Not started" },
            new() { ResidentId = 7109, ResidentName = "Noah Walsh", Suite = "E-204", RiskLevel = "High", ReviewDue = "June 16", Review = "Not started" },
            new() { ResidentId = 7110, ResidentName = "Priya Shah", Suite = "E-207", RiskLevel = "Low", ReviewDue = "June 17", Review = "Not started" },
            new() { ResidentId = 7111, ResidentName = "Ruth Carter", Suite = "E-210", RiskLevel = "High", ReviewDue = "June 18", Review = "Not started" },
            new() { ResidentId = 7112, ResidentName = "Samuel Diaz", Suite = "E-213", RiskLevel = "High", ReviewDue = "June 19", Review = "Not started" }
        };
    }
}
