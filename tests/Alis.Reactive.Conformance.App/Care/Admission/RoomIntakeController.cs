using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomIntakeController : Controller
{
    [HttpGet(CarePages.RoomIntake)]
    public IActionResult Open(RoomIntake intake) => View("/Care/Admission/RoomIntake.cshtml", intake);

    [HttpGet(CarePages.RoomIntakeOfRoom12)]
    public IActionResult OfRoom12() => View("/Care/Admission/RoomIntake.cshtml", new RoomIntake { Room = 12 });
}
