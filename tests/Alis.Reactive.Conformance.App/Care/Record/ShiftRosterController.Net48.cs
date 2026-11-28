using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftRosterController : Controller
{
    private const string PlainText = "text/plain";

    private ShiftRosterBook Book => TwinWorlds.Of(HttpContext).ShiftRoster;

    [HttpGet]
    [Address(CarePages.ShiftRoster)]
    public ActionResult ToApprove() => View("/Care/Record/ShiftRoster.Net48.cshtml", Book.Roster);

    [HttpGet]
    [Address(CarePages.ShiftRota)]
    public ActionResult Rota() => View("/Care/Record/ShiftRota.Net48.cshtml", new ShiftRota { Rows = Book.Roster.Rows });

    [HttpGet]
    [Address(CarePages.ShiftSheet)]
    public ActionResult Sheet() => View("/Care/Record/ShiftSheet.cshtml", new ShiftSheet { Rows = Book.Roster.Rows });

    [HttpGet]
    [Address(CareEndpoints.ShiftRosterNote)]
    public ActionResult Note() => PartialView("/Care/Record/ShiftRosterNote.cshtml", new ShiftNote());

    [HttpPut]
    [Address(CareEndpoints.ShiftRosterApprovals)]
    public ActionResult Approve(Roster roster)
    {
        var validation = this.Validate<Roster, RosterRules>(roster, ShiftRules.Approval);
        if (!validation.IsValid) return validation.ToErrorResponse();

        var approved = Book.Approve(roster.Rows);
        return Content($"Approved shifts {string.Join(", ", approved.Rows.Select(shift => shift.Id))}", PlainText);
    }

    [HttpPut]
    [Address(CareEndpoints.ShiftRosterShifts)]
    public ActionResult SaveShift(ShiftRow shift) => Saved(shift);

    [HttpPut]
    [Address(CareEndpoints.ShiftSheetShifts)]
    public ActionResult SaveSheetShift(ShiftRow shift) => Saved(shift);

    [HttpPut]
    [Address(CareEndpoints.ShiftRosterShiftApprovals)]
    public ActionResult ApproveShift(ShiftRow shift)
    {
        var validation = this.Validate<ShiftRow, ShiftRules>(shift, ShiftRules.Approval);
        if (!validation.IsValid) return validation.ToErrorResponse();

        Book.ApproveShift(shift);
        return Content($"Shift {shift.Id} approved", PlainText);
    }

    private ActionResult Saved(ShiftRow shift)
    {
        var validation = this.Validate<ShiftRow, ShiftRules>(shift);
        if (!validation.IsValid) return validation.ToErrorResponse();

        Book.Save(shift, Request.Form.AllKeys);
        return Content($"Shift {shift.Id} saved", PlainText);
    }
}
