using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentsBusyController : Controller
{
    private const string NoBusyLayout = "/Care/Residents/_NoBusyLayout.cshtml";
    private const string SharedLayout = "/Shared/_Layout.cshtml";

    [HttpGet(CarePages.SummaryOfOneCard)]
    public IActionResult OneCard() => View("/Care/Residents/SummaryOfOneCard.cshtml", new ResidentsSummary("North wing"));

    [HttpGet(CarePages.SummaryOfTwoCards)]
    public IActionResult TwoCards() => View("/Care/Residents/SummaryOfTwoCards.cshtml", new TwoCardsSummary("North wing"));

    [HttpGet(CareEndpoints.SummaryOfTheFirstWing)]
    [AtTheAnswerGate]
    public IActionResult FirstWing() => NoContent();

    [HttpGet(CareEndpoints.SummaryOfTheSecondWing)]
    [AtTheAnswerGate]
    public IActionResult SecondWing() => NoContent();

    [HttpGet(CarePages.CensusPanels)]
    public IActionResult Panels() => View("/Care/Residents/CensusPanels.cshtml", new CensusBoard("North wing"));

    [HttpGet(CarePages.CensusPanelsBusyInTheirPanel)]
    public IActionResult PanelsBusyInTheirPanel() => View("/Care/Residents/CensusPanelsBusyInTheirPanel.cshtml", new PanelCensusBoard("North wing"));

    [HttpGet(CareEndpoints.CensusOfTheNorthPanel)]
    [AtTheAnswerGate]
    public IActionResult NorthPanel() => Json(new PanelCensus("14"));

    [HttpGet(CareEndpoints.CensusOfTheSouthPanel)]
    [AtTheAnswerGate]
    public IActionResult SouthPanel() => Json(new PanelCensus("9"));

    [HttpGet(CareEndpoints.CensusOfTheThirdCount)]
    [AtTheAnswerGate]
    public IActionResult ThirdCount() => Json(new PanelCensus("3"));

    [HttpGet(CarePages.LoadWithNoBusyIndicator)]
    public IActionResult LoadWithNoBusyIndicator() => View("/Care/Residents/ResidentsLoad.cshtml", new ResidentsLoad("North wing", NoBusyLayout));

    [HttpGet(CarePages.LoadWithTheBusyIndicator)]
    public IActionResult LoadWithTheBusyIndicator() => View("/Care/Residents/ResidentsLoad.cshtml", new ResidentsLoad("North wing", SharedLayout));

    [HttpGet(CareEndpoints.ResidentsLoaded)]
    public IActionResult Loaded() => NoContent();

    [HttpGet(CarePages.CensusRetried)]
    public IActionResult CensusRetried() => View("/Care/Residents/CensusRetried.cshtml", new CensusRetry("North wing"));

    [HttpGet(CarePages.CensusRetriedOnce)]
    public IActionResult CensusRetriedOnce() => View("/Care/Residents/CensusRetriedOnce.cshtml", new CensusSentOnce("North wing"));

    [HttpGet(CareEndpoints.RetriedCensus)]
    [AtTheAnswerGate]
    public IActionResult RetriedCensus() => Answer(CareEndpoints.RetriedCensus, new PanelCensus("12"));

    [HttpGet(CarePages.AttemptsCounted)]
    public IActionResult Attempts() => View("/Care/Residents/AttemptsCounted.cshtml", new AttemptsCounted("North wing"));

    [HttpGet(CarePages.AttemptsCountedOnce)]
    public IActionResult AttemptsOnce() => View("/Care/Residents/AttemptsCountedOnce.cshtml", new AttemptsSentOnce("North wing"));

    [HttpGet(CareEndpoints.FailingAttempt)]
    public IActionResult FailingAttempt() => Answer(CareEndpoints.FailingAttempt, new PanelCensus("0"));

    [HttpGet(CareEndpoints.RetriedAttempt)]
    public IActionResult RetriedAttempt() => Answer(CareEndpoints.RetriedAttempt, new PanelCensus("5"));

    private IActionResult Answer(string endpoint, PanelCensus census)
    {
        var status = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.NextAnswer(endpoint);
        return status == StatusCodes.Status200OK ? Json(census) : StatusCode(status);
    }
}

public sealed record ResidentsSummary(string Wing);

public sealed record TwoCardsSummary(string Wing);

public sealed record CensusBoard(string Wing);

public sealed record PanelCensusBoard(string Wing);

public sealed record PanelCensus(string Count);

public sealed record ResidentsLoad(string Wing, string Layout);

public sealed record CensusRetry(string Wing);

public sealed record CensusSentOnce(string Wing);

public sealed record AttemptsCounted(string Wing);

public sealed record AttemptsSentOnce(string Wing);

public static class ResidentsBusyEvents
{
    public static readonly AppEvent Load = AppEvent.Declare();
    public static readonly AppEvent LoadBoth = AppEvent.Declare();
    public static readonly AppEvent LoadThird = AppEvent.Declare();
    public static readonly AppEvent LoadFirst = AppEvent.Declare();
    public static readonly AppEvent LoadSecond = AppEvent.Declare();
}
