using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitCountsController : Controller
{
    [HttpGet(CarePages.VisitCountsWithAVisitInTheNorthWard)]
    public IActionResult WithAVisitInTheNorthWard() => View("/Care/Residents/VisitCounts.cshtml", new VisitCounts(VisitCountEndpoints.WithAVisitInTheNorthWard));

    [HttpGet(CarePages.VisitCountsWithEveryVisitInTheSouthWard)]
    public IActionResult WithEveryVisitInTheSouthWard() => View("/Care/Residents/VisitCounts.cshtml", new VisitCounts(VisitCountEndpoints.WithEveryVisitInTheSouthWard));

    [HttpGet(VisitCountEndpoints.WithAVisitInTheNorthWard)]
    public IActionResult VisitsWithAVisitInTheNorthWard() => Json(VisitsWithTheSecondIn("North"));

    [HttpGet(VisitCountEndpoints.WithEveryVisitInTheSouthWard)]
    public IActionResult VisitsWithEveryVisitInTheSouthWard() => Json(VisitsWithTheSecondIn("South"));

    private static CountedVisit[] VisitsWithTheSecondIn(string ward) => [new("A", new VisitCarer("Ruth"), "South"), new("B", null, ward)];
}

public sealed record VisitCounts(string Visits);

public sealed record CountedVisit(string Name, VisitCarer? Carer, string Ward);

public sealed record VisitCarer(string Name);

public static class VisitCountEvents
{
    public static readonly AppEvent Count = AppEvent.Declare();
}

public static class VisitCountEndpoints
{
    private const string Root = CarePages.Area + "residents/visit-counts/";

    public const string WithAVisitInTheNorthWard = Root + "visits/a-visit-in-the-north-ward";

    public const string WithEveryVisitInTheSouthWard = Root + "visits/every-visit-in-the-south-ward";
}
