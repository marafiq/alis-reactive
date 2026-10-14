using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The evening med pass sheet: the medication aide works down tonight's doses on the med cart's tablet and
    /// widens the columns to read a long name in full. Every page starts from tonight's doses on file.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("ColumnFit"), ActionName("ColumnFit")]
        public IActionResult MedPassSheetPage() =>
            View("~/Areas/Sandbox/Views/Components/Fusion/Grid/ColumnFit.cshtml", new MedPassSheetViewModel());

        [HttpGet("ColumnFitDoses")]
        public IActionResult ColumnFitDoses() => Ok(new List<MedPassDose>
        {
            new MedPassDose { DoseId = 9001, Room = "214", ResidentName = "Margaret Abernathy-Whitfield", Medication = "Donepezil hydrochloride 10 mg tablet", Dose = "1 tablet", Due = "8:00 PM" },
            new MedPassDose { DoseId = 9002, Room = "216", ResidentName = "Henry Liu", Medication = "Metoprolol succinate 25 mg", Dose = "1 tablet", Due = "8:00 PM" },
            new MedPassDose { DoseId = 9003, Room = "220", ResidentName = "Rosa Diaz", Medication = "Acetaminophen 500 mg", Dose = "2 tablets", Due = "9:00 PM" },
            new MedPassDose { DoseId = 9004, Room = "222", ResidentName = "Walter Grant", Medication = "Atorvastatin 20 mg", Dose = "1 tablet", Due = "9:00 PM" }
        });
    }
}
