using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The South wing's clinical updates board: the charge nurse applies the updates the pharmacy and the
    /// therapy team sent this morning, and records what the clinical review team decided for the resident she
    /// picks. Each answer names the resident by key; every page starts from the same board, keeping nothing.
    /// </summary>
    public partial class GridController
    {
        private static readonly string[] ClinicalUpdatesRiskLevels = { "Low", "Moderate", "High" };

        [HttpGet("Operations"), ActionName("Operations")]
        public IActionResult ClinicalUpdatesPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Operations.cshtml",
                new ClinicalUpdatesViewModel { Wing = "South", RiskLevels = ClinicalUpdatesRiskLevels.ToList() });

        [HttpGet("ClinicalUpdatesBoard")]
        public IActionResult ClinicalUpdatesBoard() => Ok(ClinicalUpdatesOnFile());

        // The pharmacy added two evening med-pass tasks for Irene Morgan.
        [HttpPost("ClinicalUpdatesPharmacy")]
        public IActionResult ClinicalUpdatesPharmacy()
        {
            var irene = ClinicalUpdatesOnFile().Single(r => r.ResidentId == 7204);
            var openTasks = irene.OpenTasks + 2;
            return Ok(new ClinicalUpdatesTaskCount
            {
                ResidentId = irene.ResidentId,
                OpenTasks = openTasks,
                Summary = $"Pharmacy update applied: {irene.ResidentName} has {openTasks} open tasks."
            });
        }

        // This morning's assessment raised Grace Bennett's fall risk.
        [HttpPost("ClinicalUpdatesTherapy")]
        public IActionResult ClinicalUpdatesTherapy()
        {
            var grace = ClinicalUpdatesOnFile().Single(r => r.ResidentId == 7202);
            return Ok(new ClinicalUpdatesFallRisk
            {
                ResidentId = grace.ResidentId,
                RiskLevel = "High",
                Summary = $"Therapy update applied: {grace.ResidentName}'s fall risk is High."
            });
        }

        [HttpPost("ClinicalUpdatesReview")]
        public IActionResult ClinicalUpdatesReview([FromBody] ClinicalUpdatesReview review)
        {
            var picked = review.Residents.FirstOrDefault();
            if (picked == null)
                return BadRequest(new ClinicalUpdatesRefusal { Summary = "Pick the resident the review team saw." });
            if (!ClinicalUpdatesRiskLevels.Contains(review.ReviewRisk))
                return BadRequest(new ClinicalUpdatesRefusal { Summary = "Choose the fall risk the review team set." });
            if (review.ReviewOpenTasks is not { } openTasks)
                return BadRequest(new ClinicalUpdatesRefusal { Summary = "Enter the open tasks the review team left." });

            // A clinical review books the next one three months on.
            var nextReview = DateOnly.ParseExact(picked.NextReview, "yyyy-MM-dd", CultureInfo.InvariantCulture).AddMonths(3);
            var reviewed = new ClinicalUpdatesResident
            {
                ResidentId = picked.ResidentId,
                ResidentName = picked.ResidentName,
                Suite = picked.Suite,
                CareLevel = picked.CareLevel,
                PrimaryNurse = picked.PrimaryNurse,
                RiskLevel = review.ReviewRisk!,
                OpenTasks = (int)openTasks,
                NextReview = nextReview.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            return Ok(new ClinicalUpdatesReviewed
            {
                Resident = reviewed,
                Summary = $"Clinical review recorded for {reviewed.ResidentName}: {reviewed.RiskLevel} fall risk, "
                    + $"{reviewed.OpenTasks} open tasks, next review {nextReview.ToString("MMMM d", CultureInfo.InvariantCulture)}."
            });
        }

        private static List<ClinicalUpdatesResident> ClinicalUpdatesOnFile() => new()
        {
            new() { ResidentId = 7201, ResidentName = "Dorothy Hale", Suite = "S-101", CareLevel = "Independent", RiskLevel = "Low", OpenTasks = 2, PrimaryNurse = "Nora Ellis", NextReview = "2026-06-02" },
            new() { ResidentId = 7202, ResidentName = "Grace Bennett", Suite = "S-104", CareLevel = "Assisted Living", RiskLevel = "Moderate", OpenTasks = 3, PrimaryNurse = "Malik Stone", NextReview = "2026-06-03" },
            new() { ResidentId = 7203, ResidentName = "Harold Brooks", Suite = "S-107", CareLevel = "Memory Care", RiskLevel = "Moderate", OpenTasks = 1, PrimaryNurse = "Elena Ruiz", NextReview = "2026-06-04" },
            new() { ResidentId = 7204, ResidentName = "Irene Morgan", Suite = "S-110", CareLevel = "Skilled Nursing", RiskLevel = "Low", OpenTasks = 3, PrimaryNurse = "Owen Park", NextReview = "2026-06-05" },
            new() { ResidentId = 7205, ResidentName = "Leo Simmons", Suite = "S-112", CareLevel = "Assisted Living", RiskLevel = "Low", OpenTasks = 2, PrimaryNurse = "Nora Ellis", NextReview = "2026-06-09" },
            new() { ResidentId = 7206, ResidentName = "Katherine Ortiz", Suite = "S-115", CareLevel = "Memory Care", RiskLevel = "High", OpenTasks = 4, PrimaryNurse = "Malik Stone", NextReview = "2026-06-10" }
        };
    }
}
