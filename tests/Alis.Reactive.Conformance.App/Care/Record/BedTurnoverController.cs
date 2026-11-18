using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedTurnoverController : Controller
{
    private const RecordStage AStageNoValueNames = (RecordStage)99;

    [HttpGet(CarePages.BedTurnoverOfTheNorthWard)]
    public IActionResult OfTheNorthWard() => View("/Care/Record/BedTurnover.cshtml", new BedTurnover("North ward", BedTurnoverEndpoints.BedsOfTheNorthWard));

    [HttpGet(CarePages.BedTurnoverOfTheSouthWard)]
    public IActionResult OfTheSouthWard() => View("/Care/Record/BedTurnover.cshtml", new BedTurnover("South ward", BedTurnoverEndpoints.BedsOfTheSouthWard));

    [HttpGet(CarePages.BedTurnoverOfTheEastWard)]
    public IActionResult OfTheEastWard() => View("/Care/Record/BedTurnover.cshtml", new BedTurnover("East ward", BedTurnoverEndpoints.BedsOfTheEastWard));

    [HttpGet(CarePages.BedTurnoverOfTheWestWard)]
    public IActionResult OfTheWestWard() => View("/Care/Record/BedTurnover.cshtml", new BedTurnover("West ward", BedTurnoverEndpoints.BedsOfTheWestWard));

    [HttpGet(BedTurnoverEndpoints.BedsOfTheNorthWard)]
    public IActionResult BedsOfTheNorthWard() => Json(new TurnoverBeds(
    [
        new Record { Name = "Bed 1", Stage = RecordStage.Ready, Wing = RecordWing.South },
        new Record { Name = "Bed 2", Stage = RecordStage.Waiting, Wing = RecordWing.North },
        new Record { Name = "Bed 3", Stage = RecordStage.Closed, Wing = RecordWing.South },
    ]));

    [HttpGet(BedTurnoverEndpoints.BedsOfTheSouthWard)]
    public IActionResult BedsOfTheSouthWard() => Json(new TurnoverBeds(
    [
        new Record { Name = "Bed 1", Stage = RecordStage.Waiting, Wing = RecordWing.North },
        new Record { Name = "Bed 2", Stage = RecordStage.Ready, Wing = RecordWing.North },
        new Record { Name = "Bed 3", Stage = RecordStage.Ready, Wing = RecordWing.North },
    ]));

    [HttpGet(BedTurnoverEndpoints.BedsOfTheEastWard)]
    public IActionResult BedsOfTheEastWard() => Json(new TurnoverBeds(
    [
        new Record { Name = "Bed 1", Stage = AStageNoValueNames, Wing = RecordWing.South },
        new Record { Name = "Bed 2", Stage = RecordStage.Ready, Wing = RecordWing.North },
    ]));

    [HttpGet(BedTurnoverEndpoints.BedsOfTheWestWard)]
    public IActionResult BedsOfTheWestWard() => Json(new TurnoverBeds(
    [
        new Record { Name = "Bed 1", Stage = RecordStage.Closed, Wing = RecordWing.South },
        new Record { Name = "Bed 2", Stage = RecordStage.Ready, Wing = RecordWing.North },
    ]));
}

public sealed record BedTurnover(string Ward, string Beds);

public sealed record TurnoverBeds(IReadOnlyList<Record> Beds);

public static class BedTurnovers
{
    public static readonly AppEvent Load = AppEvent.Declare();
}

public static class BedTurnoverEndpoints
{
    private const string Root = CarePages.Area + "record/bed-turnover/";

    public const string BedsOfTheNorthWard = Root + "beds/north-ward";

    public const string BedsOfTheSouthWard = Root + "beds/south-ward";

    public const string BedsOfTheEastWard = Root + "beds/east-ward";

    public const string BedsOfTheWestWard = Root + "beds/west-ward";
}
