using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class ExecutionFeaturesController : Controller
{
    [HttpGet(BenchPages.ExecutionSpotStarts)]
    public IActionResult SpotStarts() => View("/Bench/Execution/SpotStarts.cshtml", new SpotStartsBoard(World.ComponentStartsPlace));

    [HttpGet(BenchPages.ExecutionBindingConstructors)]
    public IActionResult BindingConstructors() => View(
        "/Bench/Execution/BindingConstructors.cshtml",
        new BindingConstructorsBoard(
        [
            new BindingStart { Name = "Alpha" },
            new BindingStart { Name = "Beta", Constructor = World.ComponentBetaConstructor },
            new BindingStart { Name = "Gamma" },
            new BindingStart { Name = "Delta" },
        ]));

    [HttpGet(BenchPages.ExecutionSelfTest)]
    public IActionResult SelfTest() => View("/Bench/Execution/SelfTest.cshtml", new SelfTestBoard(90));

    [HttpGet(BenchPages.ExecutionNameFilter)]
    public IActionResult NameFilter() => View("/Bench/Execution/NameFilter.cshtml", new NameFilterBoard("Ward names"));

    [HttpGet(NameFilterEndpoints.Names)]
    public IActionResult Names(NameFilterQuery query)
        => Json(TheWardsNames().Where(name => query.Text is null || name.StartsWith(query.Text, StringComparison.OrdinalIgnoreCase)).ToList());

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private static List<string> TheWardsNames() => ["Alpha", "Alpine", "Beta"];
}

public sealed record SpotStartsBoard(string? Place);

public sealed record BindingConstructorsBoard(IReadOnlyList<BindingStart> Bindings);

public sealed record SelfTestBoard(int OldersCount);

public sealed record NameFilterBoard(string Caption);

public sealed class NameFilterQuery
{
    public string? Text { get; set; }
}

public static class NameFilterEndpoints
{
    public const string Names = BenchPages.Area + "execution/name-filter/items";
}

public static class SpotStartButtons
{
    public static readonly AppEvent ShowTheSpots = AppEvent.Declare();
}

public static class BindingConstructorButtons
{
    public static readonly AppEvent CountTheBindingsWithNoConstructor = AppEvent.Declare();
}

public static class SelfTestButtons
{
    public static readonly AppEvent RunTheSelfTest = AppEvent.Declare();

    public static readonly AppEvent ReadTheCount = AppEvent.Declare();

    public static readonly AppEvent ShowTheRemoteCode = AppEvent.Declare();

    public static readonly AppEvent ReadOldersCount = AppEvent.Declare();

    public static readonly AppEvent TakeTheMissingRecord = AppEvent.Declare();
}
