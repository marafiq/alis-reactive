using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class UnnamedRosterController : Controller
{
    private const string JsonText = "application/json";

    [HttpGet(CarePages.RosterWithAnUnnamedResident)]
    public IActionResult WithAnUnnamedResident() => View("/Care/Residents/UnnamedRoster.cshtml", new UnnamedRoster("East", UnnamedRosterEndpoints.ThreeBeds));

    [HttpGet(CarePages.RosterWithAnUnnamedAndABlankResident)]
    public IActionResult WithAnUnnamedAndABlankResident() => View("/Care/Residents/UnnamedRoster.cshtml", new UnnamedRoster("East", UnnamedRosterEndpoints.FourBeds));

    [HttpGet(UnnamedRosterEndpoints.ThreeBeds)]
    public IActionResult ThreeBeds() => Content("""[{"bed":"E1","name":""},{"bed":"E2"},{"bed":"E3","name":"Ada"}]""", JsonText);

    [HttpGet(UnnamedRosterEndpoints.FourBeds)]
    public IActionResult FourBeds() => Content("""[{"bed":"E1","name":""},{"bed":"E2"},{"bed":"E3","name":"Ada"},{"bed":"E4","name":"   "}]""", JsonText);
}

public sealed record UnnamedRoster(string Ward, string Beds);

public sealed class RosterBed
{
    public string Bed { get; set; } = "";

    public string? Name { get; set; }
}

public static class UnnamedRosterEndpoints
{
    private const string Root = CarePages.Area + "residents/roster/";

    public const string ThreeBeds = Root + "three-beds";

    public const string FourBeds = Root + "four-beds";
}

public static class UnnamedRosters
{
    public static readonly AppEvent Count = AppEvent.Declare();
}
