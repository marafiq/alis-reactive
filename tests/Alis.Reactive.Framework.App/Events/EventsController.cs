using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Events;

/// <summary>The events a component raises, and what a page's pipelines do with them (capabilities E).</summary>
public sealed class EventsController : Controller
{
    public IActionResult Slate() => View(new SlateEventsPage());
}
