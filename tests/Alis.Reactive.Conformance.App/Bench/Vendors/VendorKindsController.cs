using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class VendorKindsController : Controller
{
    [HttpGet(BenchPages.VendorsElementSignals)]
    public IActionResult ElementSignals() => View("/Bench/Vendors/ElementSignals.cshtml", new ElementSignalsBoard("12"));

    [HttpGet(BenchPages.VendorsRegisteredGauges)]
    public IActionResult RegisteredGauges() => View("/Bench/Vendors/RegisteredGauges.cshtml", new GaugesBoard(97, 88));

    [HttpGet(BenchPages.VendorsStoreUpdates)]
    public IActionResult StoreUpdates() => View("/Bench/Vendors/StoreUpdates.cshtml", new StoreUpdatesBoard("Ward updates"));

    [HttpGet(VendorCards.StoreCard)]
    public IActionResult StoreCard() => PartialView("/Bench/Vendors/StoreCard.cshtml", new StoreListeningCard("The card listens to the store."));

    [HttpGet(BenchPages.VendorsNameSearch)]
    public IActionResult NameSearch() => View("/Bench/Vendors/NameSearch.cshtml", new NameSearchBoard(["Ada Byrne", "Adam Cole"]));

    [HttpGet(BenchPages.VendorsEmptyPayloads)]
    public IActionResult EmptyPayloads() => View("/Bench/Vendors/EmptyPayloads.cshtml", new EmptyPayloadsBoard(97));

    [HttpGet(BenchPages.VendorsCallbackAndStore)]
    public IActionResult CallbackAndStore() => View("/Bench/Vendors/CallbackAndStore.cshtml", new CallbackAndStoreBoard("Room calls and ward updates"));

    [HttpGet(BenchPages.VendorsPeerChanges)]
    public IActionResult PeerChanges() => View("/Bench/Vendors/PeerChanges.cshtml", new PeerChangesBoard("Peer changes heard"));

    [HttpGet(BenchPages.VendorsClockTicks)]
    public IActionResult ClockTicks() => View("/Bench/Vendors/ClockTicks.cshtml", new ClockTicksBoard("Ticks heard"));

    [HttpGet(VendorCards.ClockCard)]
    public IActionResult ClockCard() => PartialView("/Bench/Vendors/ClockCard.cshtml", new ClockListeningCard("The card listens to the clock."));

    [HttpGet(BenchPages.VendorsBroughtComponent)]
    public IActionResult BroughtComponent() => View("/Bench/Vendors/BroughtComponent.cshtml", new BroughtComponentBoard(30m, "Ada"));

    [HttpGet(BenchPages.VendorsComponentBuiltLate)]
    public IActionResult ComponentBuiltLate() => View("/Bench/Vendors/ComponentBuilt.cshtml", new ComponentBuiltBoard(BuiltOnMount: false));

    [HttpGet(BenchPages.VendorsComponentBuiltOnMount)]
    public IActionResult ComponentBuiltOnMount() => View("/Bench/Vendors/ComponentBuilt.cshtml", new ComponentBuiltBoard(BuiltOnMount: true));
}

public sealed record ElementSignalsBoard(string Room);

public sealed record GaugesBoard(int FirstReading, int SecondReading);

public sealed record StoreUpdatesBoard(string Caption);

public sealed record StoreListeningCard(string Note);

public sealed record NameSearchBoard(IReadOnlyList<string> Names);

public sealed record EmptyPayloadsBoard(int Reading);

public sealed record CallbackAndStoreBoard(string Caption);

public sealed record PeerChangesBoard(string Caption);

public sealed record ClockTicksBoard(string Caption);

public sealed record ClockListeningCard(string Note);

public sealed record BroughtComponentBoard(decimal Amount, string RaisedFor);

public sealed record ComponentBuiltBoard(bool BuiltOnMount);

public static class VendorCards
{
    private const string Root = BenchPages.Area + "vendors/cards/";

    public const string StoreCard = Root + "store";

    public const string ClockCard = Root + "clock";
}

public static class GaugeButtons
{
    public static readonly AppEvent ReadTheFirst = AppEvent.Declare();

    public static readonly AppEvent SetTheThreshold = AppEvent.Declare();

    public static readonly AppEvent RaiseTheAlarm = AppEvent.Declare();

    public static readonly AppEvent LowerTheAlarm = AppEvent.Declare();

    public static readonly AppEvent ReadTheSecond = AppEvent.Declare();
}

public static class StoreCardButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}

public static class ClockCardButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}

public static class AmountButtons
{
    public static readonly AppEvent ReadTheAmount = AppEvent.Declare();

    public static readonly AppEvent SetFifteen = AppEvent.Declare();

    public static readonly AppEvent AddFifteen = AppEvent.Declare();
}

public static class GreetingButtons
{
    public static readonly AppEvent SayHello = AppEvent.Declare();
}
