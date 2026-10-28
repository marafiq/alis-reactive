using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Vendors;

/// <summary>The ways the framework reaches a component's object and hears its events (capabilities V).</summary>
public sealed class VendorsController : Controller
{
    public IActionResult CallbackProperties() => View(new CallbackPropertiesPage());

    public IActionResult Element() => View(new ElementPage());
}

/// <summary>A bell whose listeners are callback properties.</summary>
public sealed class CallbackPropertiesPage;

/// <summary>A meter that is itself the object, with DOM events.</summary>
public sealed class ElementPage;
