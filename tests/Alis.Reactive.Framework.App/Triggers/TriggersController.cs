using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Triggers;

/// <summary>What starts a page's pipelines: the page being ready, the URL it was opened with, and the app's own events.</summary>
public sealed class TriggersController : Controller
{
    public IActionResult Arrival() => View(new ArrivalPage());

    /// <summary>A visit card with a ready handler of its own, loaded into the arrival page.</summary>
    public IActionResult Visit() => PartialView("Visit", new VisitCard());

    public IActionResult Announcements() => View(new AnnouncementsPage());
}
