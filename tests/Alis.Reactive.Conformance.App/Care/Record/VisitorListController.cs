using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitorListController : Controller
{
    [HttpGet(CarePages.VisitorList)]
    public IActionResult OfACoachParty() => View("/Care/Record/VisitorList.cshtml", new VisitorList("Ada Hart", "Coach party of twelve", new Announcement { PartySize = 12, Capacity = 9, Type = "a coach party" }));

    [HttpGet(CarePages.VisitorListOfASmallParty)]
    public IActionResult OfASmallParty() => View("/Care/Record/VisitorList.cshtml", new VisitorList("Ada Hart", "Family of four", new Announcement { PartySize = 4, Capacity = 9, Type = "a family" }));
}

public sealed record VisitorList(string Resident, string Party, Announcement Arrival);

public sealed class Announcement
{
    public int PartySize { get; init; }

    public int Capacity { get; init; }

    public string Type { get; init; } = "";
}

public static class VisitorLists
{
    public static readonly AppEvent<Announcement> Announced = AppEvent.Declare<Announcement>();
}
