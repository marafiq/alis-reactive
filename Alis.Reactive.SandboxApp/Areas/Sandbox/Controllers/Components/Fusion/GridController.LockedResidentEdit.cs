using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Resident records: the records coordinator updates a family's phone number as the family asks. The record
    /// of a discharged resident is closed and stays as it was on the day they left. Every page starts from the
    /// records and requests on file; nothing is kept between requests.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("LockedResidentEdit"), ActionName("LockedResidentEdit")]
        public IActionResult ResidentRecordsPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/LockedResidentEdit.cshtml",
                new ResidentRecordsViewModel
                {
                    Requests = new()
                    {
                        new RecordUpdateRequest("update-record-6403", 6403, "Ruth Carter's son has a new phone number.", "Update Ruth Carter's record"),
                        new RecordUpdateRequest("update-record-6402", 6402, "Henry Liu's daughter has a new phone number.", "Update Henry Liu's record")
                    }
                });

        [HttpGet("LockedResidentEditRecords")]
        public IActionResult LockedResidentEditRecords() => Ok(new List<ResidentContactRecord>
        {
            new ResidentContactRecord { ResidentId = 6401, ResidentName = "Amina Patel", Suite = "101", Status = "Active", FamilyPhone = "555-0110" },
            new ResidentContactRecord { ResidentId = 6402, ResidentName = "Henry Liu", Suite = "108", Status = "Active", FamilyPhone = "555-0187" },
            new ResidentContactRecord { ResidentId = 6403, ResidentName = "Ruth Carter", Suite = "112", Status = "Discharged", DischargedOn = "September 30", FamilyPhone = "555-0164" },
            new ResidentContactRecord { ResidentId = 6404, ResidentName = "Grace Bennett", Suite = "115", Status = "Active", FamilyPhone = "555-0123" },
            new ResidentContactRecord { ResidentId = 6405, ResidentName = "Walter Grant", Suite = "118", Status = "Discharged", DischargedOn = "October 2", FamilyPhone = "555-0199" }
        });
    }
}
