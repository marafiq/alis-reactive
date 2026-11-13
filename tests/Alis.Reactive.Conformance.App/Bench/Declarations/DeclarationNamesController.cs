using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class DeclarationNamesController : Controller
{
    private const string JsonText = "application/json";

    [HttpGet(BenchPages.DeclarationsNamesUnderTheAppsNaming)]
    public IActionResult NamesUnderTheAppsNaming()
        => View("/Bench/Declarations/NamesUnderTheAppsNaming.cshtml", new NamesBoard(DeclarationNameEndpoints.Record, DeclarationNameEndpoints.CamelRecord));

    [HttpGet(BenchPages.DeclarationsRecordsSortedByAField)]
    public IActionResult RecordsSortedByAField() => View(
        "/Bench/Declarations/RecordsSortedByAField.cshtml",
        new SortedRecordsBoard([new RecordStart { Name = "Ada", MultiWord = 2 }, new RecordStart { Name = "Ben", MultiWord = 1 }, new RecordStart { Name = "Cy", MultiWord = 3 }]));

    [HttpGet(DeclarationNameEndpoints.Record)]
    public IActionResult Record() => Json(new MultiWordRecord(40));

    [HttpGet(DeclarationNameEndpoints.CamelRecord)]
    public IActionResult CamelRecord() => Content("{\"multiWord\": 40}", JsonText);
}

public sealed record NamesBoard(string Record, string CamelRecord);

public sealed record SortedRecordsBoard(IReadOnlyList<RecordStart> Records);

public sealed record MultiWordRecord(int MultiWord);

public static class DeclarationNameEndpoints
{
    private const string Root = BenchPages.DeclarationsNamesUnderTheAppsNaming + "/";
    public const string Record = Root + "record";
    public const string CamelRecord = Root + "camel-record";
}

public static class NameButtons
{
    public static readonly AppEvent TakeTwentyFive = AppEvent.Declare();
    public static readonly AppEvent ReadTheRecord = AppEvent.Declare();
    public static readonly AppEvent ReadTheCamelRecord = AppEvent.Declare();
    public static readonly AppEvent SortByMultiWord = AppEvent.Declare();
}
