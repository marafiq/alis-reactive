using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Triggers;

public sealed class TriggersController : Controller
{
    public IActionResult Arrival() => View(new ArrivalPage());

    public IActionResult Visit() => PartialView("Visit", new VisitCard());

    public IActionResult Announcements() => View(new AnnouncementsPage());
}
