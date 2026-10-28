using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Members;

/// <summary>The members a component declares: properties, objects and methods (capabilities P, O, M).</summary>
public sealed class MembersController : Controller
{
    public IActionResult Properties() => View(new PropertiesPage());

    public IActionResult Objects() => View(new ObjectsPage());

    public IActionResult Methods() => View(new MethodsPage());
}
