using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Framework.App.Home;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
