using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardReferralDirectoryController : Controller
{
    [HttpGet(CarePages.WardReferralDirectory)]
    public IActionResult Directory() => View("/Care/AdmissionClinical/WardReferralDirectory.cshtml", new WardReferralDirectory());
}

public sealed class WardReferralDirectory
{
    public string? Notice { get; set; }
    public string? Search { get; set; }
    public string? Choice { get; set; } = "W4";
    public bool Hold { get; set; }
    public string? Service { get; set; }
    public string? Vacancies { get; set; }
    public string? Itinerary { get; set; }
    public string? AllStops { get; set; }
    public string? Destination { get; set; }
    public IReadOnlyList<ReferralWard> Wards { get; } =
    [
        new("W4", "Willow", "Memory care", "Willow nursing office.", false),
        new("C2", "Cedar", "Recovery", "Call the Cedar charge nurse.", false),
        new("M1", "Maple", "Recovery", "Maple admissions desk.", false),
        new("O3", "Oak", "Memory care", "Oak transfer office.", false)
    ];
}

public sealed record ReferralWard(string Code, string Name, string Unit, string Instruction, bool Held);
public sealed record ReferralGroup(string Unit);

public static class ReferralDirectoryActions
{
    public static readonly AppEvent PrintVacancy = AppEvent.Declare();
    public static readonly AppEvent SearchDirections = AppEvent.Declare();
    public static readonly AppEvent PrintOrder = AppEvent.Declare();
    public static readonly AppEvent Clinical = AppEvent.Declare();
    public static readonly AppEvent Alphabetical = AppEvent.Declare();
    public static readonly AppEvent Reverse = AppEvent.Declare();
    public static readonly AppEvent ClinicalSearch = AppEvent.Declare();
    public static readonly AppEvent HandoverSearch = AppEvent.Declare();
    public static readonly AppEvent Quick = AppEvent.Declare();
    public static readonly AppEvent Urgent = AppEvent.Declare();
    public static readonly AppEvent Routine = AppEvent.Declare();
    public static readonly AppEvent Closed = AppEvent.Declare();
    public static readonly AppEvent French = AppEvent.Declare();
    public static readonly AppEvent English = AppEvent.Declare();
    public static readonly AppEvent Always = AppEvent.Declare();
    public static readonly AppEvent Floating = AppEvent.Declare();
    public static readonly AppEvent Simple = AppEvent.Declare();
}
