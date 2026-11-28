using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionWardNavigationController : Controller
{
    [HttpGet(CarePages.FusionWardNavigation)]
    public IActionResult Navigation() => View("/Care/Shell/FusionWardNavigation.cshtml", new FusionWardNavigation("Ward"));
}

public sealed record FusionWardNavigation(string MenuLabel);
