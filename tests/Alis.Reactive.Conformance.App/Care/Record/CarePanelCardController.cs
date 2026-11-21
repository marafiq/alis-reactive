using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePanelCardController : Controller
{
    [HttpGet(CarePages.CarePanelCard)]
    public IActionResult OfAResident() => View("/Care/Record/CarePanelCard.cshtml", new CarePanelCard("Ada Hart"));

    [HttpGet(CarePanelCardEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/Record/CarePanelCardPulse.cshtml", new CarePanelCardPulse("Pulse watch"));
}

public sealed record CarePanelCard(string Resident);

public sealed record CarePanelCardPulse(string Title);

public static class CarePanelCardEndpoints
{
    private const string Root = CarePages.Area + "record/care-panel-card/";

    public const string Card = Root + "card";
}

public static class CarePanelCardButtons
{
    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheCard = AppEvent.Declare();
}
