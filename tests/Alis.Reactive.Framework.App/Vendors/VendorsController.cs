using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Vendors;

public sealed class VendorsController : Controller
{
    public IActionResult CallbackProperties() => View(new CallbackPropertiesPage());

    public IActionResult Element() => View(new ElementPage());

    public IActionResult SharedCallback() => View(new SharedCallbackPage());

    public IActionResult WingListener(string wing) => PartialView("WingListener", new WingListener { Wing = wing });

    public IActionResult Unbuilt() => View(new UnbuiltPage());

    public IActionResult UnbuiltInput() => View(new UnbuiltInputPage());

    public IActionResult NewerDeclaration() => View(new NewerDeclarationPage());

    public IActionResult Refused(VendorMistake mistake) => View("Refused", new VendorRefusalsPage { Mistake = mistake });
}

public sealed class CallbackPropertiesPage;

public sealed class ElementPage;

public sealed class SharedCallbackPage;

public sealed class WingListener
{
    public string Wing { get; set; } = "";
}

public sealed class UnbuiltPage;

public sealed class UnbuiltInputPage
{
    public string? Note { get; set; }
}

public sealed class NewerDeclarationPage;
