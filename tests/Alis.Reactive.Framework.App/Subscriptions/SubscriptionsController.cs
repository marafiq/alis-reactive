using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Subscriptions;

public sealed class SubscriptionsController : Controller
{
    public IActionResult Gate() => View(new GatePage());

    public IActionResult EscortedVisit() => View(new GatePage());

    public IActionResult SignedVisit() => View(new GatePage());

    public IActionResult HeldObjectEvent() => View(new GatePage());
}
