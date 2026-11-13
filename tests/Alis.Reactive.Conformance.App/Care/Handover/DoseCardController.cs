using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseCardController : Controller
{
    private const string Card = "/Care/Handover/DoseCard.cshtml";

    [HttpGet(CarePages.DoseCardToGive)]
    public IActionResult ToGive() => View(Card, new DoseCardLabel { Content = "Give dose" });

    [HttpGet(CarePages.DoseCardToSkip)]
    public IActionResult ToSkip() => View(Card, new DoseCardLabel { Content = "Skip dose" });
}

public sealed class DoseCardLabel
{
    public required string Content { get; init; }
}

public static class DoseCardReads
{
    public static readonly AppEvent ReadLabel = AppEvent.Declare();
}
