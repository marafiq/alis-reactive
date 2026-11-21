using System.Web.Mvc;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomNumberingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.RoomNumbering)]
    public ActionResult OfARoom() => View("/Care/Admission/RoomNumbering.cshtml", new RoomNumbering());

    [HttpPost]
    [Address(RoomNumberingEndpoints.Rooms)]
    public ActionResult Save(RoomNumbering room)
    {
        var checkedRoom = new RoomNumberingRules().Validate(room);
        if (checkedRoom.IsValid) return Content(room.Number ?? string.Empty, PlainText);
        return checkedRoom.ToErrorResponse();
    }
}
