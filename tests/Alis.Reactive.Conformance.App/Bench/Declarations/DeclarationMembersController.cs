using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class DeclarationMembersController : Controller
{
    [HttpGet(BenchPages.DeclarationsOlderReadOnlyText)]
    public IActionResult OlderReadOnlyText() => View("/Bench/Declarations/OlderReadOnlyText.cshtml", new ReadOnlyTextBoard("Alpha", "RO-2026-11-10"));

    [HttpGet(BenchPages.DeclarationsPromisedPing)]
    public IActionResult PromisedPing() => View("/Bench/Declarations/PromisedPing.cshtml", new PromisedPingBoard("Alpha", PromisedIsEnabled: true));

    [HttpGet(BenchPages.DeclarationsRefusedPing)]
    public IActionResult RefusedPing() => View("/Bench/Declarations/PromisedPing.cshtml", new PromisedPingBoard("Alpha", PromisedIsEnabled: false));

    [HttpGet(BenchPages.DeclarationsItemWithoutAct)]
    public IActionResult ItemWithoutAct() => View("/Bench/Declarations/ItemWithoutAct.cshtml", new ItemActsBoard("Alpha", "One"));

    [HttpGet(BenchPages.DeclarationsRecordFields)]
    public IActionResult RecordFields() => View("/Bench/Declarations/RecordFields.cshtml", new RecordFieldsBoard("Morning"));

    [HttpGet(BenchPages.DeclarationsBoundTotals)]
    public IActionResult BoundTotals() => View(
        "/Bench/Declarations/BoundTotals.cshtml",
        new BoundTotalsBoard(25, [new ItemStart { Name = "Ada", Active = true }, new ItemStart { Name = "Bea" }, new ItemStart { Name = "Cy", Active = true }]));

    [HttpGet(BenchPages.DeclarationsMembersOffTheirDeclaration)]
    public IActionResult MembersOffTheirDeclaration() => View("/Bench/Declarations/MembersOffTheirDeclaration.cshtml", new OffDeclarationBoard("Alpha"));

    [HttpGet(BenchPages.DeclarationsReturnedItems)]
    public IActionResult ReturnedItems() => View("/Bench/Declarations/ReturnedItems.cshtml", new ReturnedItemsBoard("Alpha", "One"));

    [HttpGet(BenchPages.DeclarationsRecordCount)]
    public IActionResult RecordCount() => View("/Bench/Declarations/RecordCount.cshtml", new RecordCountBoard(TheWorldsRecords(12)));

    [HttpGet(BenchPages.DeclarationsRecordTotal)]
    public IActionResult RecordTotal() => View("/Bench/Declarations/RecordTotal.cshtml", new RecordTotalBoard(TheWorldsRecords(4)));

    private List<RecordStart> TheWorldsRecords(int held)
        => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.ComponentHoldsNoRecords
            ? []
            : [.. Enumerable.Range(1, held).Select(bed => new RecordStart { Name = $"Bed {bed}", Count = 2 })];
}

public sealed record ReadOnlyTextBoard(string Text, string ReadOnlyText);

public sealed record PromisedPingBoard(string Text, bool PromisedIsEnabled);

public sealed record ItemActsBoard(string Text, string CurrentItem);

public sealed record RecordFieldsBoard(string RecordName);

public sealed record BoundTotalsBoard(int Count, IReadOnlyList<ItemStart> Items);

public sealed record OffDeclarationBoard(string Text);

public sealed record ReturnedItemsBoard(string Text, string CurrentItem);

public sealed record RecordCountBoard(IReadOnlyList<RecordStart> Records);

public sealed record RecordTotalBoard(IReadOnlyList<RecordStart> Records);

public static class ReadOnlyTextButtons
{
    public static readonly AppEvent ReadOldersReadOnlyText = AppEvent.Declare();

    public static readonly AppEvent ReadTheReadOnlyText = AppEvent.Declare();

    public static readonly AppEvent ShowOldersText = AppEvent.Declare();
}

public static class PromisedPingButtons
{
    public static readonly AppEvent PingPromised = AppEvent.Declare();

    public static readonly AppEvent Settle = AppEvent.Declare();

    public static readonly AppEvent ShowPromisedsText = AppEvent.Declare();
}

public static class ItemActButtons
{
    public static readonly AppEvent ActOnCurrent = AppEvent.Declare();

    public static readonly AppEvent ActOnLacking = AppEvent.Declare();

    public static readonly AppEvent ShowLackingsText = AppEvent.Declare();
}

public static class RecordFieldButtons
{
    public static readonly AppEvent ShowTheRecord = AppEvent.Declare();

    public static readonly AppEvent ClearTheRecord = AppEvent.Declare();

    public static readonly AppEvent HoldTheEveningRecord = AppEvent.Declare();
}

public static class BoundTotalButtons
{
    public static readonly AppEvent ReadTheTotalTwice = AppEvent.Declare();

    public static readonly AppEvent CallTheTotalTwice = AppEvent.Declare();

    public static readonly AppEvent HoldTheTotal = AppEvent.Declare();

    public static readonly AppEvent CountTheActiveItems = AppEvent.Declare();
}

public static class OffDeclarationButtons
{
    public static readonly AppEvent LogOlder = AppEvent.Declare();

    public static readonly AppEvent EchoOlder = AppEvent.Declare();

    public static readonly AppEvent PingPromised = AppEvent.Declare();

    public static readonly AppEvent TotalPromised = AppEvent.Declare();

    public static readonly AppEvent ShowOldersText = AppEvent.Declare();

    public static readonly AppEvent ShowPromisedsText = AppEvent.Declare();

    public static readonly AppEvent TakeNothing = AppEvent.Declare();

    public static readonly AppEvent TakeTheSnapshotsName = AppEvent.Declare();

    public static readonly AppEvent TakeTheMissingItemsName = AppEvent.Declare();

    public static readonly AppEvent Ping = AppEvent.Declare();
}

public static class ReturnedItemButtons
{
    public static readonly AppEvent WriteTheValue = AppEvent.Declare();

    public static readonly AppEvent ReadTheEmptyCurrent = AppEvent.Declare();

    public static readonly AppEvent ReadTheHeldCurrent = AppEvent.Declare();

    public static readonly AppEvent ShowTheText = AppEvent.Declare();
}

public static class RecordCountButtons
{
    public static readonly AppEvent Count = AppEvent.Declare();

    public static readonly AppEvent Total = AppEvent.Declare();
}
