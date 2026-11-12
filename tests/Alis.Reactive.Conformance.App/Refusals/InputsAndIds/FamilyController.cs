using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class FamilyController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.FamilyPrefixMovedInsideItsPlan)]
    public IActionResult PrefixMovedInsideItsPlan() => View("/Refusals/InputsAndIds/FamilyPrefixMovedInsideItsPlan.cshtml", new Family());

    [HttpGet(RefusalPages.FamilyPrefixLeftAsItsPlanStarted)]
    public IActionResult PrefixLeftAsItsPlanStarted() => View("/Refusals/InputsAndIds/FamilyPrefixLeftAsItsPlanStarted.cshtml", new Family());

    [HttpPost(RefusalEndpoints.Families)]
    public IActionResult Save([FromForm] Family family)
        => Content(family.Guardian.Name is { } name ? $"Guardian: {name}" : "No guardian", PlainText);
}

public sealed class Family
{
    public Guardian Guardian { get; set; } = new();
}

public sealed class Guardian
{
    public string? Name { get; set; }
}

public static class Families
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
