using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Move-in day: the admissions coordinator moves today's arrivals onto the North wing roster and today's
    /// departures off it. Every page starts from the same roster; the roster changes only in the browser.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("ResidentAdmissionAudit"), ActionName("ResidentAdmissionAudit")]
        public IActionResult MoveInDayPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ResidentAdmissionAudit.cshtml",
                new MoveInDayViewModel
                {
                    Roster = new()
                    {
                        new MoveInDayResident { ResidentId = 6001, ResidentName = "Amina Patel", CareLevel = "Memory Care", Suite = "N-101", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 6002, ResidentName = "Henry Liu", CareLevel = "Assisted Living", Suite = "N-112", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 6003, ResidentName = "Grace Bennett", CareLevel = "Independent", Suite = "N-114", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 6004, ResidentName = "Irene Morgan", CareLevel = "Skilled Nursing", Suite = "N-116", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 6005, ResidentName = "Jonah Reed", CareLevel = "Assisted Living", Suite = "N-118", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 6006, ResidentName = "Mara Thompson", CareLevel = "Memory Care", Suite = "N-122", AgreementSigned = true }
                    },
                    Arrivals = new()
                    {
                        new MoveInDayResident { ResidentId = 9101, ResidentName = "Zara Ahmed", CareLevel = "Assisted Living", Suite = "N-120", AgreementSigned = true },
                        new MoveInDayResident { ResidentId = 9102, ResidentName = "Walter Price", CareLevel = "Memory Care", Suite = "N-124", AgreementSigned = false }
                    },
                    Departures = new()
                    {
                        new MoveInDayDeparture(6002, "Henry Liu", "N-112", "moving in with his daughter")
                    }
                });
    }
}
