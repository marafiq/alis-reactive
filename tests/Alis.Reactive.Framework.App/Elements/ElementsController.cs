using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Elements;

public sealed class ElementsController : Controller
{
    public IActionResult Panel() => View(new PanelPage());

    public IActionResult Refused(DispatchMistake mistake) => View(new RefusedDispatchPage { Mistake = mistake });
}
