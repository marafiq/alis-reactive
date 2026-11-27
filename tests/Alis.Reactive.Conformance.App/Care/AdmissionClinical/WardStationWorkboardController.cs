using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardStationWorkboardController : Controller
{
    [HttpGet(CarePages.WardStationWorkboard)]
    public IActionResult Board() => View("/Care/AdmissionClinical/WardStationWorkboard.cshtml", new WardStationWorkboard());
}

public sealed class WardStationWorkboard
{
    public string? Ward { get; set; }
    public IReadOnlyList<StationWard> Wards { get; } =
    [
        new("Maple", "214", "Use the charge desk."), new("Cedar", "319", "Use the south entrance."),
        new("Willow", "425", "Meet at the garden lift."), new("Oak", "416", "Use the rehab desk."),
        new("Hazel", "521", "Meet the nursing lead."), new("Elm", "512", "Use the main entrance."),
        new("Beech", "604", "Call the night nurse."), new("Ash", "228", "Use the rehab entrance."),
        new("Birch", "302", "Ask for the ward clerk."), new("Chestnut", "308", "Use the east lift."),
        new("Juniper", "414", "Meet at the care desk."), new("Rowan", "510", "Call the receiving team."),
        new("Aspen", "518", "Use the main entrance."), new("Laurel", "601", "Meet the unit coordinator."),
        new("Linden", "614", "Meet the receiving nurse at the east entrance.")
    ];
}

public sealed record StationWard(string Name, string Extension, string Instructions);

public static class WardStationActions
{
    public static readonly AppEvent RosterFirst = AppEvent.Declare();
    public static readonly AppEvent AlertFirst = AppEvent.Declare();
    public static readonly AppEvent Alert = AppEvent.Declare();
    public static readonly AppEvent FullWidth = AppEvent.Declare();
    public static readonly AppEvent PocketWidth = AppEvent.Declare();
    public static readonly AppEvent Tall = AppEvent.Declare();
    public static readonly AppEvent Short = AppEvent.Declare();
    public static readonly AppEvent Personal = AppEvent.Declare();
    public static readonly AppEvent Shared = AppEvent.Declare();
    public static readonly AppEvent Prepare = AppEvent.Declare();
    public static readonly AppEvent Reminders = AppEvent.Declare();
}
