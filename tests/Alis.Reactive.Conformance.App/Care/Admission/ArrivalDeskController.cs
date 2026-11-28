using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ArrivalDeskController : Controller
{
    private const string PlainText = "text/plain";
    private const string TakenRoom = "12";
    private const string RoomToClean = "14";
    private const string IsolationWard = "East";
    private const string ThisDesk = "front";

    [HttpGet(CarePages.ArrivalDesk)]
    public IActionResult OfTheDesk() => View("/Care/Admission/ArrivalDesk.cshtml", new ArrivalDesk { Desk = ThisDesk });

    [HttpGet(ArrivalDeskEndpoints.Rooms)]
    public IActionResult Rooms([FromQuery] ArrivalRoomsQuery query) => PartialView("/Care/Admission/ArrivalDeskRooms.cshtml", new ArrivalDesk { Ward = query.Ward });

    [HttpGet(ArrivalDeskEndpoints.Funding)]
    public IActionResult Funding() => PartialView("/Care/Admission/ArrivalDeskFunding.cshtml", new ArrivalDesk());

    [HttpGet(ArrivalDeskEndpoints.Kin)]
    public IActionResult Kin() => PartialView("/Care/Admission/ArrivalDeskKin.cshtml", new ArrivalKin());

    [HttpPost(ArrivalDeskEndpoints.KinSaves)]
    public IActionResult SaveKin([FromForm] ArrivalKin kin)
    {
        var checkedKin = new ArrivalKinRules().Validate(kin);
        if (checkedKin.IsValid) return Content($"Next of kin: {kin.Name}", PlainText);
        return checkedKin.ToErrorResponse();
    }

    [HttpPost(ArrivalDeskEndpoints.Arrivals)]
    public IActionResult Arrive([FromForm] ArrivalDesk arrival, [FromHeader(Name = ArrivalDeskEndpoints.WardHeader)] string? ward, [FromQuery] bool rehearsal)
    {
        var checkedArrival = new ArrivalDeskRules().Validate(arrival);
        if (!checkedArrival.IsValid) return checkedArrival.ToErrorResponse();
        if (arrival.Room == TakenRoom) return Conflict(new ArrivalConflict(new ArrivalHolder("Ann Lee", arrival.Desk == ThisDesk)));
        var bed = new ArrivalBed(new ArrivalRoom(arrival.Room ?? string.Empty), arrival.Room == RoomToClean, ward == IsolationWard);
        return Json(new ArrivalReply(bed, rehearsal));
    }

    [HttpGet(ArrivalDeskEndpoints.Cleaning)]
    public IActionResult Cleaning([FromQuery] ArrivalCleaningQuery query)
        => Content(query.Deep ? $"Deep clean booked for room {query.Room}" : $"Clean booked for room {query.Room}", PlainText);

    [HttpGet(ArrivalDeskEndpoints.BedCard)]
    public IActionResult BedCard([FromQuery] ArrivalBedCardQuery query) => PartialView("/Care/Admission/ArrivalDeskBedCard.cshtml", new ArrivalBedCard(query.Room ?? string.Empty));

    [HttpGet(ArrivalDeskEndpoints.Holder)]
    public IActionResult Holder() => Content("Ann Lee arrived at this desk today", PlainText);

    [HttpGet(ArrivalDeskEndpoints.Census)]
    public IActionResult Census() => Content("18 residents", PlainText);

    [HttpGet(ArrivalDeskEndpoints.Alerts)]
    public IActionResult Alerts() => Content("2 alerts", PlainText);
}

public sealed class ArrivalDesk
{
    public ArrivalResident Resident { get; set; } = new();

    public string? Ward { get; set; }

    public string? Room { get; set; }

    public bool Funded { get; set; }

    public string? FundingNumber { get; set; }

    public string? Desk { get; set; }

    public bool Rehearsal { get; set; }
}

public sealed class ArrivalResident
{
    public string? Name { get; set; }
}

public sealed class ArrivalKin
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

public sealed record ArrivalRoomsQuery(string? Ward);

public sealed record ArrivalCleaningQuery(string? Room, bool Deep);

public sealed record ArrivalBedCardQuery(string? Room);

public sealed record ArrivalBedCard(string Room);

public sealed record ArrivalReply(ArrivalBed Bed, bool Rehearsal);

public sealed record ArrivalBed(ArrivalRoom Room, bool NeedsCleaning, bool Isolation);

public sealed record ArrivalRoom(string Number);

public sealed record ArrivalConflict(ArrivalHolder Holder);

public sealed record ArrivalHolder(string Resident, bool AtThisDesk);

public sealed class ArrivalDeskRules : ReactiveValidator<ArrivalDesk>
{
    private const string AlreadyArrived = "Ada Lee";

    public ArrivalDeskRules()
    {
        RuleFor(arrival => arrival.Resident.Name).NotEmpty().WithMessage("Name the resident.");
        RuleFor(arrival => arrival.Resident.Name).Must(name => name != AlreadyArrived).WithMessage("Ada Lee has arrived already.");
        RuleFor(arrival => arrival.Ward).NotEmpty().WithMessage("Choose a ward.");
        RuleFor(arrival => arrival.Room).NotEmpty().WithMessage("Choose a room.");
        RuleFor(arrival => arrival.FundingNumber).NotEmpty().WithMessage("Give the funding number.").When(arrival => arrival.Funded);
    }
}

public sealed class ArrivalKinRules : ReactiveValidator<ArrivalKin>
{
    public ArrivalKinRules()
    {
        RuleFor(kin => kin.Name).NotEmpty().WithMessage("Name the next of kin.");
        RuleFor(kin => kin.Phone).NotEmpty().WithMessage("Give a phone number.");
    }
}

public static class ArrivalDeskLists
{
    public static IReadOnlyList<SelectListItem> Wards() => [new SelectListItem("North", "North"), new SelectListItem("East", "East")];

    public static IReadOnlyList<SelectListItem> RoomsOf(string? ward) => ward == "East"
        ? [new SelectListItem("Room 14", "14"), new SelectListItem("Room 15", "15")]
        : [new SelectListItem("Room 11", "11"), new SelectListItem("Room 12", "12")];
}

public static class ArrivalDeskEndpoints
{
    public const string WardHeader = "Care-Ward";

    public const string Rooms = CarePages.ArrivalDesk + "/rooms";

    public const string Funding = CarePages.ArrivalDesk + "/funding";

    public const string Kin = CarePages.ArrivalDesk + "/kin";

    public const string KinSaves = CarePages.ArrivalDesk + "/kin/saves";

    public const string Arrivals = CarePages.ArrivalDesk + "/arrivals";

    public const string Cleaning = CarePages.ArrivalDesk + "/cleaning";

    public const string BedCard = CarePages.ArrivalDesk + "/bed-card";

    public const string Holder = CarePages.ArrivalDesk + "/holder";

    public const string Census = CarePages.ArrivalDesk + "/census";

    public const string Alerts = CarePages.ArrivalDesk + "/alerts";
}

public static class ArrivalDeskButtons
{
    public static readonly AppEvent Arrive = AppEvent.Declare();

    public static readonly AppEvent AddKin = AppEvent.Declare();

    public static readonly AppEvent SaveKin = AppEvent.Declare();

    public static readonly AppEvent RefreshBoard = AppEvent.Declare();
}
