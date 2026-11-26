using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftRosterController : Controller
{
    private const string PlainText = "text/plain";

    private ShiftRosterBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(ShiftRosterBook.Open);

    [HttpGet(CarePages.ShiftRoster)]
    public IActionResult ToApprove() => View("/Care/Record/ShiftRoster.cshtml", Book.Roster);

    [HttpGet(CarePages.ShiftRota)]
    public IActionResult Rota() => View("/Care/Record/ShiftRota.cshtml", new ShiftRota { Rows = Book.Roster.Rows });

    [HttpGet(CarePages.ShiftSheet)]
    public IActionResult Sheet() => View("/Care/Record/ShiftSheet.cshtml", new ShiftSheet { Rows = Book.Roster.Rows });

    [HttpGet(CareEndpoints.ShiftRosterNote)]
    public IActionResult Note() => PartialView("/Care/Record/ShiftRosterNote.cshtml", new ShiftNote());

    [HttpPut(CareEndpoints.ShiftRosterApprovals)]
    public IActionResult Approve([FromForm] Roster roster)
    {
        var validation = this.Validate<Roster, RosterRules>(roster, ShiftRules.Approval);
        if (!validation.IsValid) return validation.ToErrorResponse();

        var approved = Book.Approve(roster.Rows);
        return Content($"Approved shifts {string.Join(", ", approved.Rows.Select(shift => shift.Id))}", PlainText);
    }

    [HttpPut(CareEndpoints.ShiftRosterShifts)]
    public IActionResult SaveShift([FromForm] ShiftRow shift) => Saved(shift);

    [HttpPut(CareEndpoints.ShiftSheetShifts)]
    public IActionResult SaveSheetShift([FromForm] ShiftRow shift) => Saved(shift);

    [HttpPut(CareEndpoints.ShiftRosterShiftApprovals)]
    public IActionResult ApproveShift([FromForm] ShiftRow shift)
    {
        var validation = this.Validate<ShiftRow, ShiftRules>(shift, ShiftRules.Approval);
        if (!validation.IsValid) return validation.ToErrorResponse();

        Book.ApproveShift(shift);
        return Content($"Shift {shift.Id} approved", PlainText);
    }

    private IActionResult Saved(ShiftRow shift)
    {
        var validation = this.Validate<ShiftRow, ShiftRules>(shift);
        if (!validation.IsValid) return validation.ToErrorResponse();

        Book.Save(shift, Request.Form.Keys);
        return Content($"Shift {shift.Id} saved", PlainText);
    }
}

internal sealed partial class ShiftRosterBook : IOwnedByAWorld
{
    void IOwnedByAWorld.Take(Seed seed)
    {
    }
}
