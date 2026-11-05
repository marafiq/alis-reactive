using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Events;

public sealed class EventsController : Controller
{
    public IActionResult Slate() => View(new SlateEventsPage());
}
