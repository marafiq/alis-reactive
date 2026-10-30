using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Elements;

/// <summary>The page's own elements, the app's one-per-page component, another model's component, and dispatch.</summary>
public sealed class ElementsController : Controller
{
    public IActionResult Panel() => View(new PanelPage());

    public IActionResult Refused(DispatchMistake mistake) => View(new RefusedDispatchPage { Mistake = mistake });
}
