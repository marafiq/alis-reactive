using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomTypeController : Controller
{
    [HttpGet(CarePages.RoomType)]
    public IActionResult OfAResident() => View("/Care/AdmissionClinical/RoomType.cshtml", new RoomTypeChoice("Rose Alvarez"));
}

public sealed record RoomTypeChoice(string Resident);

public static class RoomTypeChecks
{
    public static readonly AppEvent ShowRoom = AppEvent.Declare();
}
