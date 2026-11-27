using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionWardTrailController : Controller
{
    [HttpGet(CarePages.FusionWardTrail)]
    public IActionResult Wards() => View("/Care/Shell/FusionWardTrail.cshtml", new FusionWardTrail(CarePages.FusionWardTrail));

    [HttpGet(CarePages.FusionNorthWingTrail)]
    public IActionResult NorthWing() => View("/Care/Shell/FusionWardTrail.cshtml", new FusionWardTrail(CarePages.FusionNorthWingTrail));
}

public sealed record FusionWardTrail(string ActiveItem);

public static class FusionWardTrailButtons
{
    public static readonly AppEvent WhereAmI = AppEvent.Declare();
}
