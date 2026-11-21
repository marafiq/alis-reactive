using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomSaveController : Controller
{
    private const string PlainText = "text/plain";
    private const string ClosedRoom = "13";

    [HttpGet(CarePages.RoomSave)]
    public IActionResult OfAResident() => View("/Care/Admission/RoomSave.cshtml", new RoomSave());

    [HttpGet(RoomSaveEndpoints.Room)]
    public IActionResult Room() => PartialView("/Care/Admission/RoomSaveRoom.cshtml", new RoomSave());

    [HttpPost(RoomSaveEndpoints.Saves)]
    public async Task<IActionResult> Save([FromForm] RoomSave save)
    {
        await World.Reach(GateName.Of(OwnershipGate.ShowsFailures), Request.Path);
        if (save.Room == ClosedRoom) ModelState.AddModelError(nameof(RoomSave.Room), $"Room {ClosedRoom} is closed.");
        if (ModelState.IsValid) return Content($"Room {save.Room} saved", PlainText);
        return ValidationProblem(ModelState);
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
}

public sealed class RoomSave
{
    public string? Room { get; set; }
}

public static class RoomSaveEndpoints
{
    public const string Room = CarePages.Area + "admission/room-save/room";

    public const string Saves = CarePages.Area + "admission/room-save/saves";
}

public static class RoomSaveButtons
{
    public static readonly AppEvent Save = AppEvent.Declare();

    public static readonly AppEvent ReloadTheRoom = AppEvent.Declare();
}
