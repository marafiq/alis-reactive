using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ShiftCardController : Controller
{
    [HttpGet(CarePages.ShiftCard)]
    public IActionResult WithItsRadio() => View("/Care/Record/ShiftCard.cshtml", new ShiftCard("Night shift", [RecordScripts.CardRadio]));

    [HttpGet(CarePages.ShiftCardWithAStuckRadio)]
    public IActionResult WithAStuckRadio() => View("/Care/Record/ShiftCard.cshtml", new ShiftCard("Night shift", [RecordScripts.CardRadioWithAStuckUnsubscribe]));

    [HttpGet(ShiftCardEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/Record/ShiftCardSheet.cshtml", new ShiftCardSheet("Handover card"));

    [HttpPost(ShiftCardEndpoints.Save)]
    public IActionResult Save() => NoContent();
}

public sealed record ShiftCard(string Shift, IReadOnlyList<string> Scripts) : IScriptedRecordPage;

public sealed record ShiftCardSheet(string Title);

public static class ShiftCardEndpoints
{
    private const string Root = CarePages.Area + "record/shift-card/";

    public const string Card = Root + "card";

    public const string Save = Root + "saves";
}

public static class ShiftCardButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent SaveTheCard = AppEvent.Declare();
}
