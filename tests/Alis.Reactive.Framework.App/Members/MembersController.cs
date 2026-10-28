using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Members;

/// <summary>The members a component declares: properties, objects and methods (capabilities P, O, M).</summary>
public sealed class MembersController : Controller
{
    public IActionResult Properties() => View(new PropertiesPage());

    public IActionResult Objects() => View(new ObjectsPage());

    public IActionResult Methods() => View(new MethodsPage());

    public IActionResult Data() => View(new DataPage());

    public IActionResult Kinds() => View(new KindsPage());

    public IActionResult Headed() => View(new HeadedPage());

    public IActionResult Names() => View(new NamesPage());

    public IActionResult Refused(MemberMistake mistake) => View("Refused", new MemberRefusalsPage { Mistake = mistake });
}
