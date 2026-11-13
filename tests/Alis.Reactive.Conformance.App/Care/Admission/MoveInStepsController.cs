using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoveInStepsController : Controller
{
    [HttpGet(CarePages.MoveInSteps)]
    public IActionResult OfTheHome() => View("/Care/Admission/MoveInSteps.cshtml", new MoveInSteps("Rose House", new MoveInResidentStep()));

    [HttpGet(MoveInStepsEndpoints.RoomStep)]
    public IActionResult RoomStep() => PartialView("/Care/Admission/MoveInRoomStep.cshtml", new MoveInRoomStep("Room 12 is ready."));
}

public sealed record MoveInSteps(string Home, MoveInResidentStep ResidentStep);

public sealed class MoveInResidentStep
{
    public string? Resident { get; set; }
}

public sealed record MoveInRoomStep(string Note);

public sealed class MoveInRing
{
    public int Count { get; init; }
}

public sealed class MoveInBell : Plugin
{
    public MoveInBell()
        : base("move-in-bell")
    {
    }

    public MethodResult<int> Ring() => Method<int>();
}

public static class MoveInStepsEndpoints
{
    private const string Root = CarePages.Area + "admission/move-in-steps/";

    public const string RoomStep = Root + "room";
}

public static class MoveInStepsButtons
{
    public static readonly AppEvent RingTheBell = AppEvent.Declare();

    public static readonly AppEvent<MoveInRing> BellRang = AppEvent.Declare<MoveInRing>();

    public static readonly AppEvent Next = AppEvent.Declare();
}
