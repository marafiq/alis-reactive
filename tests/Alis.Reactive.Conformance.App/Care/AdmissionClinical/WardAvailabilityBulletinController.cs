using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardAvailabilityBulletinController : Controller
{
    [HttpGet(CarePages.WardAvailabilityBulletin)]
    public IActionResult Desk([FromQuery] bool personal = false)
        => View("/Care/AdmissionClinical/WardAvailabilityBulletin.cshtml", new WardAvailabilityBulletin { PersonalStation = personal });

    [HttpGet(CareEndpoints.WardAvailability)]
    public async Task<IActionResult> Wards()
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        await world.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        var status = world.NextAnswer(CareEndpoints.WardAvailability);
        return status == StatusCodes.Status200OK ? Json(WardAvailabilityBulletin.OnCall) : StatusCode(status);
    }
}

public sealed class WardAvailabilityBulletin
{
    public string? OnCallWard { get; set; }
    public string? Published { get; set; }
    public bool Withhold { get; set; }
    public bool RequiresReview { get; set; }
    public string? Workstation { get; set; } = "M1";
    public bool PersonalStation { get; init; }
    public string? Card { get; set; } = "M1";
    public string? OpenReview { get; set; }
    public bool PanelHeld { get; set; }
    public string? Provisional { get; set; } = "M1";
    public string? Confirmed { get; set; } = "C2";
    public string? Badge { get; set; }
    public double? BedCode { get; set; } = 0;
    public bool? EscortFlag { get; set; } = false;
    public string? Service { get; set; }
    public static IReadOnlyList<BulletinWard> PublishedWards { get; } = [new("M1", "Maple", false), new("C2", "Cedar", true)];
    public static IReadOnlyList<BulletinWard> OnCall { get; } = [new("O3", "Oak", true), new("W4", "Willow", false), new("H5", "Hazel", false)];
    public IReadOnlyList<BedReferral<double>> Beds { get; } = [new(0, "Maple observation bed"), new(7, "Cedar transfer bed")];
    public IReadOnlyList<BedReferral<bool>> Escorts { get; } = [new(false, "Maple independent bay"), new(true, "Cedar assisted bay")];
}

public sealed record BulletinWard(string Code, string Label, bool Secure);
public sealed record BedReferral<T>(T Key, string Name);

public static class WardBulletinActions
{
    public static readonly AppEvent PrintCard = AppEvent.Declare();
    public static readonly AppEvent Load = AppEvent.Declare();
    public static readonly AppEvent Publish = AppEvent.Declare();
    public static readonly AppEvent Remember = AppEvent.Declare();
    public static readonly AppEvent Shared = AppEvent.Declare();
    public static readonly AppEvent Wide = AppEvent.Declare();
    public static readonly AppEvent Compact = AppEvent.Declare();
    public static readonly AppEvent Open = AppEvent.Declare();
    public static readonly AppEvent RichBadges = AppEvent.Declare();
    public static readonly AppEvent ShortBadges = AppEvent.Declare();
    public static readonly AppEvent Provisional = AppEvent.Declare();
    public static readonly AppEvent Confirmed = AppEvent.Declare();
    public static readonly AppEvent ZeroBed = AppEvent.Declare();
    public static readonly AppEvent NumberedBed = AppEvent.Declare();
    public static readonly AppEvent MissingBed = AppEvent.Declare();
}
