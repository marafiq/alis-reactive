using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion;

public partial class DropDownListController
{
    [HttpGet("ResidentAssignment")]
    public IActionResult ResidentAssignment(int? residentId, string operation = "read")
    {
        ViewBag.Operation = operation;
        return View("~/Areas/Sandbox/Views/Components/Fusion/DropDownList/ResidentAssignment.cshtml",
            new ResidentAssignmentModel { ResidentId = residentId, BackupResidentId = residentId });
    }

    [HttpPost("AssignResident")]
    public IActionResult AssignResident([FromBody] ResidentAssignmentModel assignment)
    {
        if (assignment.ResidentId is null)
            return BadRequest(new AssignmentReceipt("Choose a resident before saving the assignment."));

        var name = assignment.ResidentId == 1042 ? "Helen Park" : "Arthur Bell";
        return Ok(new AssignmentReceipt($"{name} assigned to {assignment.Wing} wing."));
    }
}
