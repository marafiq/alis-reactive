using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Vendors;

/// <summary>
/// The ways the framework reaches a component's object and hears its events (capabilities V), and where it
/// meets a component that is not what the page expects: not built yet, or older than its declaration.
/// </summary>
public sealed class VendorsController : Controller
{
    public IActionResult CallbackProperties() => View(new CallbackPropertiesPage());

    public IActionResult Element() => View(new ElementPage());

    public IActionResult SharedCallback() => View(new SharedCallbackPage());

    /// <summary>A wing's listener of the page's bell, loaded into the wing's slot.</summary>
    public IActionResult WingListener(string wing) => PartialView("WingListener", new WingListener { Wing = wing });

    public IActionResult Unbuilt() => View(new UnbuiltPage());

    public IActionResult NewerDeclaration() => View(new NewerDeclarationPage());
}

/// <summary>A bell whose listeners are callback properties.</summary>
public sealed class CallbackPropertiesPage;

/// <summary>A meter that is itself the object, with DOM events.</summary>
public sealed class ElementPage;

/// <summary>A bell whose callback property the page's handlers and two wings' handlers all listen through.</summary>
public sealed class SharedCallbackPage;

/// <summary>A wing that listens to the page's bell while its slot holds it.</summary>
public sealed class WingListener
{
    public string Wing { get; set; } = "";
}

/// <summary>A slate whose element its script has not built a slate on.</summary>
public sealed class UnbuiltPage;

/// <summary>A slate written against a declaration newer than the slate the page runs.</summary>
public sealed class NewerDeclarationPage;
