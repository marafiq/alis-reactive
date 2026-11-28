using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomNumberingController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoomNumbering)]
    public IActionResult OfARoom() => View("/Care/Admission/RoomNumbering.cshtml", new RoomNumbering());

    [HttpPost(RoomNumberingEndpoints.Rooms)]
    public IActionResult Save([FromForm] RoomNumbering room) => Content(room.Number ?? string.Empty, PlainText);
}
