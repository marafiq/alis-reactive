using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class VendorSlotsController : Controller
{
    [HttpGet(BenchPages.VendorsCallbackListeners)]
    public IActionResult CallbackListeners() => View("/Bench/Vendors/CallbackListeners.cshtml", new CallbackListenersBoard("Room calls"));

    [HttpGet(VendorSlotCards.CallbackCard)]
    public IActionResult CallbackCard() => PartialView("/Bench/Vendors/CallbackCard.cshtml", new CallbackCard("The card listens to the room calls."));

    [HttpGet(VendorSlotCards.InnerCard)]
    public IActionResult InnerCard() => PartialView("/Bench/Vendors/ListeningCallbacksCard.cshtml", new ListeningCallbacksCard("The inner card listens to the room calls.", "callbacks", "inner"));

    [HttpGet(VendorSlotCards.Notes)]
    public IActionResult Notes() => PartialView("/Bench/Vendors/ListeningCallbacksCard.cshtml", new ListeningCallbacksCard("The notes listen to the room calls.", "callbacks", "notes"));

    [HttpGet(BenchPages.VendorsCallbackHolder)]
    public IActionResult CallbackHolder() => View("/Bench/Vendors/CallbackHolder.cshtml", new CallbackHolderBoard("Room calls", [VendorsScripts.PageTally]));

    [HttpGet(VendorSlotCards.Holder)]
    public IActionResult Holder() => PartialView("/Bench/Vendors/CallbackHolderCard.cshtml", new CallbackHolderCard("The holder listens to the room calls."));

    [HttpGet(VendorSlotCards.HolderInner)]
    public IActionResult HolderInner() => PartialView("/Bench/Vendors/CallbackHolderInner.cshtml", new CallbackHolderInner("The inner slot listens to the room calls."));

    [HttpGet(BenchPages.VendorsCallbackScript)]
    public IActionResult CallbackScript() => View("/Bench/Vendors/CallbackScript.cshtml", new CallbackScriptBoard("Room calls"));

    [HttpGet(VendorSlotCards.Card)]
    public IActionResult Card() => PartialView("/Bench/Vendors/ListeningCallbacksCard.cshtml", new ListeningCallbacksCard("The card listens to the room calls.", "callbacks", "card"));

    [HttpGet(BenchPages.VendorsFrozenCallbacks)]
    public IActionResult FrozenCallbacks() => View("/Bench/Vendors/FrozenCallbacks.cshtml", new FrozenCallbacksBoard("Room calls of two wings"));

    [HttpGet(VendorSlotCards.CardA)]
    public IActionResult CardA() => PartialView("/Bench/Vendors/ListeningCallbacksCard.cshtml", new ListeningCallbacksCard("Card A listens to wing A's calls.", "callbacks-a", "card-a"));

    [HttpGet(VendorSlotCards.CardB)]
    public IActionResult CardB() => PartialView("/Bench/Vendors/ListeningCallbacksCard.cshtml", new ListeningCallbacksCard("Card B listens to wing B's calls.", "callbacks-b", "card-b"));

    [HttpGet(VendorSlotCards.WingNotes)]
    public IActionResult WingNotes() => PartialView("/Bench/Vendors/FrozenCallbacksNotes.cshtml", new FrozenCallbacksNotes("The notes listen to wing A when asked."));

    [HttpGet(BenchPages.VendorsRestartedCallbacks)]
    public IActionResult RestartedCallbacks() => View("/Bench/Vendors/RestartedCallbacks.cshtml", new RestartedCallbacksBoard("Room calls"));

    [HttpGet(BenchPages.VendorsRestartedComponent)]
    public IActionResult RestartedComponent() => View("/Bench/Vendors/RestartedComponent.cshtml", new RestartedComponentBoard("Raises heard", [VendorsScripts.PageTally]));

    [HttpGet(VendorSlotCards.RaisesHolder)]
    public IActionResult RaisesHolder() => PartialView("/Bench/Vendors/RestartedComponentHolder.cshtml", new RestartedComponentHolder("The holder listens to the component's raises."));

    [HttpGet(BenchPages.VendorsCollectedPeer)]
    public IActionResult CollectedPeer() => View("/Bench/Vendors/CollectedPeer.cshtml", new CollectedPeerBoard("Peer changes heard", [VendorsScripts.PageTally]));

    [HttpGet(VendorSlotCards.PeerHolder)]
    public IActionResult PeerHolder() => PartialView("/Bench/Vendors/CollectedPeerHolder.cshtml", new CollectedPeerHolder("The holder listens to the peer each raise hands."));
}

public sealed record CallbackListenersBoard(string Caption);

public sealed record CallbackCard(string Note);

public sealed record ListeningCallbacksCard(string Note, string Callbacks, string Word);

public sealed record CallbackHolderBoard(string Caption, IReadOnlyList<string> Scripts) : IScriptedVendorsPage;

public sealed record CallbackHolderCard(string Note);

public sealed record CallbackHolderInner(string Note);

public sealed record CallbackScriptBoard(string Caption);

public sealed record FrozenCallbacksBoard(string Caption);

public sealed record FrozenCallbacksNotes(string Note);

public sealed record RestartedCallbacksBoard(string Caption);

public sealed record RestartedComponentBoard(string Caption, IReadOnlyList<string> Scripts) : IScriptedVendorsPage;

public sealed record RestartedComponentHolder(string Note);

public sealed record CollectedPeerBoard(string Caption, IReadOnlyList<string> Scripts) : IScriptedVendorsPage;

public sealed record CollectedPeerHolder(string Note);

public static class VendorSlotCards
{
    private const string Root = BenchPages.Area + "vendors/slot-cards/";

    public const string CallbackCard = Root + "callback-card";

    public const string InnerCard = Root + "inner-card";

    public const string Notes = Root + "notes";

    public const string Holder = Root + "holder";

    public const string HolderInner = Root + "holder-inner";

    public const string Card = Root + "card";

    public const string CardA = Root + "card-a";

    public const string CardB = Root + "card-b";

    public const string WingNotes = Root + "wing-notes";

    public const string RaisesHolder = Root + "raises-holder";

    public const string PeerHolder = Root + "peer-holder";
}

public static class CallbackListenersButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent OpenTheNotes = AppEvent.Declare();

    public static readonly AppEvent CloseTheNotes = AppEvent.Declare();
}

public static class CallbackCardButtons
{
    public static readonly AppEvent OpenTheInnerCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheInnerCard = AppEvent.Declare();
}

public static class CallbackHolderButtons
{
    public static readonly AppEvent OpenTheSlot = AppEvent.Declare();

    public static readonly AppEvent CloseTheSlot = AppEvent.Declare();

    public static readonly AppEvent OpenTheInnerSlot = AppEvent.Declare();
}

public static class CallbackScriptButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}

public static class FrozenCallbacksButtons
{
    public static readonly AppEvent OpenCardA = AppEvent.Declare();

    public static readonly AppEvent OpenCardB = AppEvent.Declare();

    public static readonly AppEvent CloseCardB = AppEvent.Declare();

    public static readonly AppEvent OpenTheNotes = AppEvent.Declare();

    public static readonly AppEvent ListenToA = AppEvent.Declare();
}

public static class RestartedCallbacksButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}

public static class RestartedComponentButtons
{
    public static readonly AppEvent OpenTheSlot = AppEvent.Declare();

    public static readonly AppEvent CloseTheSlot = AppEvent.Declare();
}

public static class CollectedPeerButtons
{
    public static readonly AppEvent OpenTheSlot = AppEvent.Declare();

    public static readonly AppEvent CloseTheSlot = AppEvent.Declare();

    public static readonly AppEvent CheckTheFirstPeer = AppEvent.Declare();
}
