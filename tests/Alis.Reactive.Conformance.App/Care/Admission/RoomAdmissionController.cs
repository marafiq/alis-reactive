using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomAdmissionController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoomAdmission)]
    public IActionResult OfAResident() => View("/Care/Admission/RoomAdmission.cshtml", new RoomAdmission());

    [HttpGet(RoomAdmissionEndpoints.RoomLine)]
    public IActionResult RoomLine() => PartialView("/Care/Admission/RoomAdmissionRoomLine.cshtml", new RoomAdmission());

    [HttpPost(RoomAdmissionEndpoints.Admissions)]
    public IActionResult Admit([FromForm] RoomAdmission admission) => Content($"{admission.Resident} admitted to room {admission.Room}", PlainText);
}

public sealed class RoomAdmission
{
    public string? Resident { get; set; }

    public string? Room { get; set; }
}

public sealed class RoomAdmissionRules : ReactiveValidator<RoomAdmission>
{
    public RoomAdmissionRules() => RuleFor(admission => admission.Room).NotEmpty().WithMessage("Choose the resident's room.");
}

public static class RoomAdmissionEndpoints
{
    public const string RoomLine = CarePages.Area + "admission/room-admission/room-line";

    public const string Admissions = CarePages.Area + "admission/room-admission/admissions";
}

public static class RoomAdmissionButtons
{
    public static readonly AppEvent ChooseARoom = AppEvent.Declare();

    public static readonly AppEvent ClearTheRoom = AppEvent.Declare();

    public static readonly AppEvent Admit = AppEvent.Declare();
}
