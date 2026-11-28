using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RecordCardController : Controller
{
    [HttpGet(CarePages.RecordCard)]
    public IActionResult WithASummary() => View("/Care/Record/RecordCard.cshtml", new RecordCard("Ada Hart", "B-12", RecordCardEndpoints.Summary));

    [HttpGet(CarePages.RecordCardWithNoSummary)]
    public IActionResult WithNoSummary() => View("/Care/Record/RecordCard.cshtml", new RecordCard("Ada Hart", "B-12", RecordCardEndpoints.SummaryOfNoRound));

    [HttpGet(RecordCardEndpoints.Mark)]
    public IActionResult Mark() => Json(new CardMark(null));

    [HttpGet(RecordCardEndpoints.Summary)]
    public IActionResult Summary() => Json(new CardSummary("<strong>Stable</strong> overnight"));

    [HttpGet(RecordCardEndpoints.SummaryOfNoRound)]
    public IActionResult SummaryOfNoRound() => Json(new CardSummary(null));
}

public sealed record RecordCard(string Resident, string Room, string Summary);

public sealed record CardMark(string? Mark);

public sealed record CardSummary(string? Markup);

public static class RecordCardEndpoints
{
    private const string Root = CarePages.Area + "record/record-card/";

    public const string Mark = Root + "mark";

    public const string Summary = Root + "summary";

    public const string SummaryOfNoRound = Root + "summary-of-no-round";
}

public static class RecordCards
{
    public static readonly AppEvent Flag = AppEvent.Declare();

    public static readonly AppEvent ClearFlag = AppEvent.Declare();

    public static readonly AppEvent ToggleFlag = AppEvent.Declare();

    public static readonly AppEvent MarkTheRound = AppEvent.Declare();

    public static readonly AppEvent LoadSummary = AppEvent.Declare();
}
