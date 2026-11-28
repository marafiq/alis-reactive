using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ConvertedNamesController : Controller
{
    [HttpGet(RefusalPages.ConvertedNamesCounted)]
    public IActionResult ConvertedCounted() => View("/Refusals/Execution/ConvertedNamesCounted.cshtml", new SpotRoster("East wing"));

    [HttpGet(RefusalPages.WrittenNamesCounted)]
    public IActionResult WrittenCounted() => View("/Refusals/Execution/WrittenNamesCounted.cshtml", new SpotRoster("East wing"));

    [HttpGet(RefusalPages.ConvertedNameFound)]
    public IActionResult ConvertedFound() => View("/Refusals/Execution/ConvertedNameFound.cshtml", new SpotRoster("East wing"));

    [HttpGet(RefusalPages.WrittenNameFound)]
    public IActionResult WrittenFound() => View("/Refusals/Execution/WrittenNameFound.cshtml", new SpotRoster("East wing"));

    [HttpGet(ExecutionEndpoints.WrittenBindings)]
    public IActionResult WrittenBindings() => Json(new List<WrittenBinding>
    {
        new("Alpha", new WrittenSpot { Name = "Ruth" }),
        new("Beta", new WrittenSpot { Name = "Ruth" }),
        new("Gamma", new WrittenSpot { Name = "Sam" }),
    });
}

public sealed record SpotRoster(string Wing);

public sealed record WrittenBinding(string Name, WrittenSpot Converted);

public sealed class WrittenSpot
{
    public string Name { get; init; } = "";
}

public static class ExecutionEndpoints
{
    public const string WrittenBindings = RefusalPages.Area + "execution/written-names/bindings";
}

public static class ConvertedNames
{
    public static readonly AppEvent Count = AppEvent.Declare();

    public static readonly AppEvent Show = AppEvent.Declare();
}
