using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WideDrawerController : Controller
{
    [HttpGet(CarePages.WideDrawer)]
    public IActionResult OfTheWard() => View("/Care/Shell/WideDrawer.cshtml", new WideDrawerPage("North"));
}

public sealed record WideDrawerPage(string Ward);

public static class WideDrawerButtons
{
    public static readonly AppEvent OpenWide = AppEvent.Declare();

    public static readonly AppEvent ReadSize = AppEvent.Declare();
}
