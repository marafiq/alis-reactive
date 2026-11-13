using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentRoomController : Controller
{
    private const string RoomName = "/Care/RecordContacts/ResidentRoom.cshtml";

    [HttpGet(CarePages.ResidentRoomNamed)]
    public IActionResult Named() => View(RoomName, new ResidentRoom { Room = "North 12" });

    [HttpGet(CarePages.ResidentRoomUnnamed)]
    public IActionResult Unnamed() => View(RoomName, new ResidentRoom());
}

public sealed class ResidentRoom
{
    public string? Room { get; set; }
}
