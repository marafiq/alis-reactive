using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInDeskController
{
    [HttpGet(CareEndpoints.MoveInReceipts)]
    public IActionResult Receipt([FromQuery] ReceiptQuery query)
    {
        if (DropsItsConnection()) return new EmptyResult();

        return PartialView(Views + "Receipt.cshtml", Desk.ReceiptOf(query.DraftId, query.Reference));
    }
}
