using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Boundaries;

/// <summary>
/// Where a plan meets what the framework did not write (grammar.md 3.4): a component answering with a
/// promise where its declaration says it answers at once, a component keeping another kind of value where
/// its declaration says list, and a page loading the runtime its own way.
/// </summary>
public sealed class BoundariesController : Controller
{
    public IActionResult Desk() => View(new DeskPage());

    /// <summary>A page of its own HTML whose bundles are deferred, so they run after the document is parsed.</summary>
    public IActionResult Late() => View(new DeskPage());

    /// <summary>A page of its own HTML that loads the runtime and no bundle of components or plugins.</summary>
    public IActionResult Bare() => View(new DeskPage());
}
