using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomIntakeController : Controller
{
    [HttpGet]
    [Address(CarePages.RoomIntake)]
    public ActionResult Open(RoomIntake intake) => View("/Care/Admission/RoomIntake.cshtml", intake);

    [HttpGet]
    [Address(CarePages.RoomIntakeOfRoom12)]
    public ActionResult OfRoom12() => View("/Care/Admission/RoomIntake.cshtml", new RoomIntake { Room = 12 });
}
