using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Poc.App.Home;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
