using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DiningCompanionsController : Controller
{
    [HttpGet(CarePages.DiningCompanions)]
    public IActionResult AtLunch() => View("/Care/RecordContacts/DiningCompanions.cshtml", new DiningCompanions());
}

public sealed class DiningCompanions;

public static class DiningSeats
{
    public static readonly AppEvent WithCarey = AppEvent.Declare();

    public static readonly AppEvent Everyone = AppEvent.Declare();
}
