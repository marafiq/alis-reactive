using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The morning huddle sheet: the staffing scheduler chooses which columns it carries, in what order, and
    /// what the wing column is called, before it prints. Every page starts from the same five residents.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("CareStaffColumns"), ActionName("CareStaffColumns")]
        public IActionResult HuddleSheetPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/CareStaffColumns.cshtml",
                new HuddleSheetViewModel { WingHeading = "Wing" });

        [HttpGet("HuddleSheetResidents")]
        public IActionResult HuddleSheetResidents() => Ok(new List<HuddleSheetResident>
        {
            new() { ResidentId = 7301, ResidentName = "Ada Lawson", RiskLevel = "Low", PrimaryNurse = "Nora Ellis", NextReview = "2026-06-02", Wing = "North", CareLevel = "Independent" },
            new() { ResidentId = 7302, ResidentName = "Bernard Cole", RiskLevel = "High", PrimaryNurse = "Malik Stone", NextReview = "2026-06-04", Wing = "North", CareLevel = "Memory Care" },
            new() { ResidentId = 7303, ResidentName = "Clara Webb", RiskLevel = "Moderate", PrimaryNurse = "Elena Ruiz", NextReview = "2026-06-05", Wing = "East", CareLevel = "Assisted Living" },
            new() { ResidentId = 7304, ResidentName = "Desmond Fry", RiskLevel = "Low", PrimaryNurse = "Owen Park", NextReview = "2026-06-08", Wing = "East", CareLevel = "Skilled Nursing" },
            new() { ResidentId = 7305, ResidentName = "Edith Moore", RiskLevel = "Moderate", PrimaryNurse = "Nora Ellis", NextReview = "2026-06-09", Wing = "South", CareLevel = "Assisted Living" }
        });
    }
}
