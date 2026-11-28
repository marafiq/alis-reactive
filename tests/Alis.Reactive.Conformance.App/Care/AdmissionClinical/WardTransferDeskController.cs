using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardTransferDeskController : Controller
{
    [HttpGet(CarePages.WardTransferDesk)]
    public IActionResult Desk() => View("/Care/AdmissionClinical/WardTransferDesk.cshtml", new WardTransferDesk());

    [HttpPost(WardTransferDeskActions.Receipt)]
    public IActionResult Handover([FromForm] WardTransferDesk transfer)
    {
        var route = transfer.Route switch { "Lift" => "Lift team", "Courtyard" => "Outdoor escort", _ => "Route pending" };
        var notice = transfer.NoticeMinutes is { } minutes ? $"{minutes} minutes" : "Notice pending";
        var escort = transfer.Escort switch { true => "Escort reserved", false => "Independent travel", _ => "Escort pending" };
        return Content($"{route} / {notice} / {escort}", "text/plain");
    }
}

public sealed class WardTransferDesk
{
    public string? Destination { get; set; }
    public string? Route { get; set; }
    public int? NoticeMinutes { get; set; }
    public bool? Escort { get; set; }
    public string? Overflow { get; set; }
    public string? Directory { get; set; }
    public string? BedAssignment { get; set; } = "M1";
    public string? Confirmation { get; set; }
    public bool OpeningHeld { get; set; }
    public bool KeepOpen { get; set; }

    public IReadOnlyList<TransferWard> Wards { get; } =
    [
        new("M1", "Maple", "Report to the Maple charge nurse.", "Secure care", false),
        new("C2", "Cedar", "Report to the Cedar admission desk.", "Assisted care", true),
    ];

    public IReadOnlyList<TransferWard> ReleasedWards { get; } =
    [
        new("O3", "Oak", "Report to the Oak lounge.", "Secure care", false),
        new("W4", "Willow", "Report to the Willow nurses' station.", "Secure care", false),
    ];

    public IReadOnlyList<ReceivingTeam> Teams { get; } =
    [
        new("W4", "Willow", new("Night lead at extension 214.")),
        new("C2", "Cedar", new("Day lead at extension 319.")),
    ];
}

public sealed record TransferWard(string Code, string Label, string Instruction, string Service, bool Held);
public sealed record ReceivingTeam(string Code, string Label, TeamContact Contact);
public sealed record TeamContact(string Handover);
public sealed record TransferChoice<T>([property: JsonPropertyName("value")] T Value, [property: JsonPropertyName("text")] T Text);

public static class WardTransferDeskActions
{
    public const string Receipt = CarePages.WardTransferDesk + "/handover";
    public static readonly AppEvent Save = AppEvent.Declare();
    public static readonly AppEvent RemoveDestination = AppEvent.Declare();
    public static readonly AppEvent MapleReferral = AppEvent.Declare();
    public static readonly AppEvent CedarReferral = AppEvent.Declare();
    public static readonly AppEvent UnknownReferral = AppEvent.Declare();
    public static readonly AppEvent ReleaseOak = AppEvent.Declare();
    public static readonly AppEvent PrioritizeOak = AppEvent.Declare();
    public static readonly AppEvent ReleaseBoth = AppEvent.Declare();
    public static readonly AppEvent PrioritizeBoth = AppEvent.Declare();
    public static readonly AppEvent OpenConfirmation = AppEvent.Declare();
    public static readonly AppEvent CloseConfirmation = AppEvent.Declare();
}
