using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomFreeRecordController : Controller
{
    [HttpGet(CarePages.RoomFreeRecordWithTheRoomFree)]
    public IActionResult WithTheRoomFree() => View("/Care/Record/RoomFreeRecord.cshtml", new RoomFreeRecord { RoomFree = true });

    [HttpGet(CarePages.RoomFreeRecordWithTheRoomTaken)]
    public IActionResult WithTheRoomTaken() => View("/Care/Record/RoomFreeRecord.cshtml", new RoomFreeRecord { RoomFree = false });
}

public sealed class RoomFreeRecord
{
    public bool RoomFree { get; set; }
}
