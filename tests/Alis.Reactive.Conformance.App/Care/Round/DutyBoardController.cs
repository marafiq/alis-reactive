using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DutyBoardController : Controller
{
    [HttpGet(CarePages.DutyBoardWithANurseWithNoName)]
    public IActionResult WithANurseWithNoName() => View("/Care/Round/DutyBoard.cshtml", new DutyBoard(DutyBoardEndpoints.NurseWithNoName));

    [HttpGet(CarePages.DutyBoardWithBeaOnDuty)]
    public IActionResult WithBeaOnDuty() => View("/Care/Round/DutyBoard.cshtml", new DutyBoard(DutyBoardEndpoints.Bea));

    [HttpGet(DutyBoardEndpoints.NurseWithNoName)]
    public IActionResult NurseWithNoName() => Json(new NurseOnDuty());

    [HttpGet(DutyBoardEndpoints.Bea)]
    public IActionResult Bea() => Json(new NurseOnDuty { Name = "Bea" });
}

public sealed record DutyBoard(string Nurse);

public sealed class NurseOnDuty
{
    public string? Name { get; init; }
}

public static class DutyBoardEndpoints
{
    private const string Root = CarePages.Area + "round/duty-board/";
    public const string NurseWithNoName = Root + "nurse/no-name";
    public const string Bea = Root + "nurse/bea";
}

public static class DutyBoards
{
    public static readonly AppEvent FindTheNurse = AppEvent.Declare();
}
