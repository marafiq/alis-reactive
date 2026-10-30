using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Subscriptions;

/// <summary>
/// What a page's handlers subscribe to beyond its components: the document's events, raised by a pipeline
/// or by markup the page carries, and a stream's own events, never an object the stream holds.
/// </summary>
public sealed class SubscriptionsController : Controller
{
    public IActionResult Gate() => View(new GatePage());

    /// <summary>Markup that raises a payload holding a record, refused as the view renders.</summary>
    public IActionResult EscortedVisit() => View(new GatePage());

    public IActionResult HeldObjectEvent() => View(new GatePage());
}
