using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusItemChecksController : Controller
{
    [HttpGet(CarePages.CensusItemChecks)]
    public IActionResult ItemChecks() => View("/Care/Residents/CensusItemChecks.cshtml", new CensusItemChecks(
        [new ItemStart { Name = "Hall" }, new ItemStart { Name = "Bath" }],
        [new ItemStart { Name = "Lounge" }, new ItemStart { Name = "Garden", Active = true }, new ItemStart { Name = "Annex", Active = true }]));
}

public sealed record CensusItemChecks(IReadOnlyList<ItemStart> Census, IReadOnlyList<ItemStart> North);

public static class CensusChecks
{
    public static readonly AppEvent Census = AppEvent.Declare();

    public static readonly AppEvent North = AppEvent.Declare();
}
