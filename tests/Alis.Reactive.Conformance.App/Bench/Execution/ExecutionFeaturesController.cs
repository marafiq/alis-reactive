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

    [HttpGet(BenchPages.ExecutionBindingDefaults)]
    public IActionResult BindingDefaults() => View("/Bench/Execution/BindingDefaults.cshtml", new BindingDefaultsBoard(BindingDefaultsEndpoints.Bindings, BindingDefaultsEndpoints.Binding));

    [HttpGet(BindingDefaultsEndpoints.Bindings)]
    public IActionResult DefaultedBindings() => Json(new[]
    {
        new DefaultedBinding("Alpha", "A-1", 0, "own"),
        new DefaultedBinding("Beta", "B-2", World.BindingsBetaCount, World.BindingsBetaStarted),
        new DefaultedBinding("Gamma", "G-3", 2, "start"),
    });

    [HttpGet(BindingDefaultsEndpoints.Binding)]
    public IActionResult DefaultedBindingAlone() => Json(new DefaultedBinding("Delta", "D-4", 0, World.BindingStarted));

    [HttpGet(BenchPages.ExecutionRequiredBindings)]
    public IActionResult RequiredBindings() => View("/Bench/Execution/RequiredBindings.cshtml", new RequiredBindingsBoard(RequiredBindingsEndpoints.Bindings, RequiredBindingsEndpoints.Binding));

    [HttpGet(RequiredBindingsEndpoints.Bindings)]
    public IActionResult BindingsMissingACode() => Json(new object[]
    {
        new { Name = "Alpha", Code = "A-1", Count = 0 },
        new { Name = "Beta", Count = 0 },
    });

    [HttpGet(RequiredBindingsEndpoints.Binding)]
    public IActionResult BindingMissingItsCount() => Json(new { Name = "Gamma", Code = "G-3" });

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

public sealed record BindingDefaultsBoard(string Bindings, string Binding);

public sealed record DefaultedBinding(string Name, string Code, int Count, string? Started);

public sealed record RequiredBindingsBoard(string Bindings, string Binding);

public sealed class NameFilterQuery
{
    public string? Text { get; set; }
}

public static class NameFilterEndpoints
{
    public const string Names = BenchPages.Area + "execution/name-filter/items";
}

public static class BindingDefaultsEndpoints
{
    public const string Bindings = BenchPages.ExecutionBindingDefaults + "/bindings";

    public const string Binding = BenchPages.ExecutionBindingDefaults + "/binding";
}

public static class RequiredBindingsEndpoints
{
    public const string Bindings = BenchPages.ExecutionRequiredBindings + "/bindings";

    public const string Binding = BenchPages.ExecutionRequiredBindings + "/binding";
}

public static class BindingDefaultsButtons
{
    public static readonly AppEvent CountTheZeroCounts = AppEvent.Declare();

    public static readonly AppEvent CountTheStarted = AppEvent.Declare();

    public static readonly AppEvent ShowBetasStarted = AppEvent.Declare();

    public static readonly AppEvent ShowTheBindingsStarted = AppEvent.Declare();
}

public static class RequiredBindingsButtons
{
    public static readonly AppEvent CountTheZeroCounts = AppEvent.Declare();

    public static readonly AppEvent ShowGammasCount = AppEvent.Declare();
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
