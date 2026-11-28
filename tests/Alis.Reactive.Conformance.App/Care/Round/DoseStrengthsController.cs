using System.Globalization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseStrengthsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DoseStrengths)]
    public IActionResult OfTheBoard() => View("/Care/Round/DoseStrengths.cshtml", new DoseStrengths { Desk = DoseStrengthEndpoints.BostonDesk });

    [HttpGet(CarePages.DoseStrengthsWithGermanDesk)]
    public IActionResult WithGermanDesk() => View("/Care/Round/DoseStrengths.cshtml", new DoseStrengths { Desk = DoseStrengthEndpoints.GermanDesk });

    [HttpGet(DoseStrengthEndpoints.BostonDeskRoute)]
    public IActionResult BostonDesk() => PartialView("/Care/Round/DoseDesk.cshtml", new DoseDesk { Save = DoseStrengthEndpoints.BostonStrength });

    [HttpGet(DoseStrengthEndpoints.GermanDeskRoute)]
    public IActionResult GermanDesk() => PartialView("/Care/Round/DoseDesk.cshtml", new DoseDesk { Save = DoseStrengthEndpoints.GermanStrength });

    [HttpPost(DoseStrengthEndpoints.BoardStrengthRoute)]
    public IActionResult SaveBoard([FromForm] DoseStrengths board) => Content(board.Strength.ToString(CultureInfo.InvariantCulture), PlainText);

    [HttpPost(DoseStrengthEndpoints.DeskStrengthRoute)]
    public IActionResult SaveDesk([FromForm] DoseDesk desk) => Content(desk.Strength.ToString(CultureInfo.InvariantCulture), PlainText);
}

public sealed class DoseStrengths
{
    public decimal Strength { get; set; }

    public string Desk { get; init; } = "";
}

public sealed class DoseDesk
{
    public decimal Strength { get; set; }

    public string Save { get; init; } = "";
}

public static class DoseStrengthEndpoints
{
    public const string BostonDeskRoute = CarePages.DoseStrengths + "/boston-desk";
    public const string BostonDesk = BostonDeskRoute + "?culture=en-US";
    public const string GermanDeskRoute = CarePages.DoseStrengths + "/desk-in-german";
    public const string GermanDesk = GermanDeskRoute + "?culture=de-DE";
    public const string BoardStrengthRoute = CarePages.DoseStrengths + "/board-strength";
    public const string BoardStrength = BoardStrengthRoute + "?culture=de-DE";
    public const string DeskStrengthRoute = CarePages.DoseStrengths + "/desk-strength";
    public const string BostonStrength = DeskStrengthRoute + "?culture=en-US";
    public const string GermanStrength = DeskStrengthRoute + "?culture=de-DE";
}

public static class DoseStrengthButtons
{
    public static readonly AppEvent OpenBostonDesk = AppEvent.Declare();

    public static readonly AppEvent SaveBoard = AppEvent.Declare();

    public static readonly AppEvent SaveDesk = AppEvent.Declare();
}
