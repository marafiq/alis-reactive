using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentSummaryCardController : Controller
{
    [HttpGet(CarePages.ResidentSummaryCard)]
    public IActionResult OfTheWing() => View("/Care/Residents/ResidentSummaryCard.cshtml", new ResidentSummaryCard("North wing"));

    [HttpGet(ResidentSummaryCardEndpoints.Summary)]
    public IActionResult Summary()
    {
        if (!HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.SummaryDropsItsConnection) return Json(new ResidentCardSummary { Resident = "Ada Hart" });

        HttpContext.Abort();
        return new EmptyResult();
    }
}

public sealed record ResidentSummaryCard(string Wing);

public sealed class ResidentCardSummary
{
    public string Resident { get; set; } = "";
}

public static class ResidentSummaryCardEndpoints
{
    public const string Summary = CarePages.ResidentSummaryCard + "/summary";
}

public static class ResidentSummaryCards
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
