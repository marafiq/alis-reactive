using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusFirstActiveItemsController : Controller
{
    [HttpGet(CarePages.CensusFirstActiveItems)]
    public IActionResult FirstActiveItems() => View("/Care/Residents/CensusFirstActiveItems.cshtml", new CensusFirstActiveItems(
        [new ItemStart { Name = "Hall" }, new ItemStart { Name = "Bath" }],
        [new ItemStart { Active = true, LacksName = true }]));
}

public sealed record CensusFirstActiveItems(IReadOnlyList<ItemStart> Census, IReadOnlyList<ItemStart> South);

public static class CensusFirstActiveItemEvents
{
    public static readonly AppEvent Census = AppEvent.Declare();

    public static readonly AppEvent South = AppEvent.Declare();
}
