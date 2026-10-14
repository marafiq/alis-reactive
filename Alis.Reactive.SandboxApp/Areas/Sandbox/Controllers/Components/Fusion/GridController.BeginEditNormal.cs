using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Care reviews: the wellness nurse opens each resident's review from the list. Opening a review checks
    /// the record out to the nurse; the server answers with its word and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("BeginEditNormalRoster")]
        public IActionResult BeginEditNormalRoster() => Ok(CareReviewsOnFile());

        [HttpPost("BeginEditNormalCheckout")]
        public IActionResult BeginEditNormalCheckout([FromBody] CareReviewCheckout? checkout)
        {
            var resident = CareReviewsOnFile().FirstOrDefault(r => r.ResidentId == checkout?.ResidentId);
            if (resident == null)
                return NotFound(new CareReviewCheckedOut { Summary = "That record is not on the care review list." });

            // The census sheet numbers its lines from one.
            return Ok(new CareReviewCheckedOut
            {
                Summary = $"{resident.ResidentName}'s record is checked out to you for review (census line {checkout!.CensusLine + 1})."
            });
        }

        private static List<CareReviewResident> CareReviewsOnFile() => new()
        {
            new CareReviewResident { ResidentId = 6301, ResidentName = "Amina Patel", Suite = "101", RiskLevel = "Low", OpenTasks = 2 },
            new CareReviewResident { ResidentId = 6302, ResidentName = "Grace Bennett", Suite = "104", RiskLevel = "Moderate", OpenTasks = 3, HoldReason = "Hospital transfer to Mercy General" },
            new CareReviewResident { ResidentId = 6303, ResidentName = "Henry Liu", Suite = "108", RiskLevel = "Moderate", OpenTasks = 6 },
            new CareReviewResident { ResidentId = 6304, ResidentName = "Irene Morgan", Suite = "112", RiskLevel = "High", OpenTasks = 4 }
        };
    }
}
