using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
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

public sealed class RoomNumbering
{
    public string? Number { get; set; }
}

public sealed class RoomNumberingRules : ReactiveValidator<RoomNumbering>
{
    public RoomNumberingRules() => RuleFor(room => room.Number).Matches(@"^[0-9]+$").WithMessage("Write the room number in digits 0 to 9.");
}

public static class RoomNumberingEndpoints
{
    public const string Rooms = CarePages.RoomNumbering + "/rooms";
}
