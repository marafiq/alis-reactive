using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyTipController : Controller
{
    private const string Tip = "/Care/RecordContacts/AllergyTip.cshtml";

    [HttpGet(CarePages.AllergyTipOfPenicillin)]
    public IActionResult OfPenicillin() => View(Tip, new AllergyTip { Tip = "Allergic to penicillin" });

    [HttpGet(CarePages.AllergyTipOfNone)]
    public IActionResult OfNone() => View(Tip, new AllergyTip { Tip = "No known allergies" });
}

public sealed class AllergyTip
{
    public required string Tip { get; init; }
}

public static class AllergyTips
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
