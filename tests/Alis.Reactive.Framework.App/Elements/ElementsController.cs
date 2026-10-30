using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Elements;

/// <summary>The page's own elements, the app's one-per-page component, another model's component, and dispatch.</summary>
public sealed class ElementsController : Controller
{
    public IActionResult Panel() => View(new PanelPage());

    /// <summary>The round board, its element and its slate named in the order <paramref name="first"/> says.</summary>
    public IActionResult RoundBoard(BoardStep first) => View(new RoundBoardPage { First = first });

    public IActionResult Refused(DispatchMistake mistake) => View(new RefusedDispatchPage { Mistake = mistake });
}
