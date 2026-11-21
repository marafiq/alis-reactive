using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInReceipts)]
    public IActionResult Receipt([FromQuery] ReceiptQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();

        if (ReviewAccess(query.DraftId) is { } refusal) return refusal;
        return Desk.ReceiptOf(query.DraftId, query.Reference) is { } receipt ? PartialView(Views + "Receipt.cshtml", receipt) : NotFound(MoveInErrors.Missing());
    }
}
