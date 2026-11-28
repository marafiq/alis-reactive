using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class VendorAnswersController : Controller
{
    [HttpGet(BenchPages.VendorsAnswers)]
    public IActionResult Answers() => View("/Bench/Vendors/Answers.cshtml", new AnswersBoard(TheThreeResidents()));

    [HttpGet(BenchPages.VendorsLiveAnswers)]
    public IActionResult LiveAnswers() => View("/Bench/Vendors/LiveAnswers.cshtml", new LiveAnswersBoard("", "Day", TheThreeResidents(), [new ItemStart { Name = "Ada" }]));

    [HttpGet(BenchPages.VendorsAnswerAskedByAWrite)]
    public IActionResult AnswerAskedByAWrite() => View("/Bench/Vendors/AnswerAskedByAWrite.cshtml", new AnswerAskedByAWriteBoard("Ward 3", [new ItemStart { Name = "Ada" }]));

    [HttpGet(BenchPages.VendorsAnswerAskedByTheComponent)]
    public IActionResult AnswerAskedByTheComponent() => View("/Bench/Vendors/AnswerAskedByTheComponent.cshtml", new AnswerAskedByTheComponentBoard("Ward 3", [new ItemStart { Name = "Ada" }]));

    [HttpGet(BenchPages.VendorsPairedAnswers)]
    public IActionResult PairedAnswers() => View(
        "/Bench/Vendors/PairedAnswers.cshtml",
        new PairedAnswersBoard([new RecordStart { Name = "Ada", Count = 2 }, new RecordStart { Name = "Bea", Count = 4 }, new RecordStart { Name = "Cy", Count = 1 }]));

    private static List<ItemStart> TheThreeResidents() => [new ItemStart { Name = "Ada", Value = 2 }, new ItemStart { Name = "Bea", Value = 4 }, new ItemStart { Name = "Cy", Value = 1 }];
}

public sealed record AnswersBoard(IReadOnlyList<ItemStart> Items);

public sealed record LiveAnswersBoard(string UnitName, string Shift, IReadOnlyList<ItemStart> CaptionItems, IReadOnlyList<ItemStart> FooterItems);

public sealed record AnswerAskedByAWriteBoard(string Ward, IReadOnlyList<ItemStart> Items);

public sealed record AnswerAskedByTheComponentBoard(string Ward, IReadOnlyList<ItemStart> Items);

public sealed record PairedAnswersBoard(IReadOnlyList<RecordStart> Records);

public static class LiveAnswerButtons
{
    public static readonly AppEvent SetTheTextToNight = AppEvent.Declare();

    public static readonly AppEvent SortByValue = AppEvent.Declare();
}

public static class WardTextButtons
{
    public static readonly AppEvent SetTheTextNorth = AppEvent.Declare();

    public static readonly AppEvent SetTheTextSouth = AppEvent.Declare();
}
