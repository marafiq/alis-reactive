using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RegionalWardDirectoryController : Controller
{
    [HttpGet(CarePages.RegionalWardDirectory)]
    public IActionResult Directory() => View("/Care/AdmissionClinical/RegionalWardDirectory.cshtml", new RegionalWardDirectory());
}

public sealed class RegionalWardDirectory
{
    public string? RegionalWard { get; set; }
    public string? EmergencyWard { get; set; }
    public string? PolicyRoute { get; set; }
    public double? PolicyNotice { get; set; }
    public bool? PolicyEscort { get; set; }
    public string? ReleasedRoute { get; set; }
    public double? ReleasedNotice { get; set; }
    public bool? ReleasedEscort { get; set; }
    public IReadOnlyList<string> Wards { get; } = Enumerable.Range(1, 150)
        .Select(number => "District ward " + number.ToString("000", CultureInfo.InvariantCulture)).ToArray();
}

public static class RegionalWardActions
{
    public static readonly AppEvent Paged = AppEvent.Declare();
    public static readonly AppEvent Full = AppEvent.Declare();
    public static readonly AppEvent Routine = AppEvent.Declare();
    public static readonly AppEvent Isolate = AppEvent.Declare();
    public static readonly AppEvent Normal = AppEvent.Declare();
    public static readonly AppEvent Release = AppEvent.Declare();
    public static readonly AppEvent Prioritize = AppEvent.Declare();
}
