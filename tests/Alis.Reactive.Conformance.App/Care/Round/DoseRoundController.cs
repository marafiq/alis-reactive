using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseRoundController : Controller
{
    private const string JsonText = "application/json";

    [HttpGet(CarePages.DoseRoundWithAWardlessDose)]
    public IActionResult WithAWardlessDose() => View("/Care/Round/DoseRound.cshtml", new DoseRound("Morning", DoseRoundEndpoints.DosesWithAWardlessDose));

    [HttpGet(CarePages.DoseRoundWithEveryDoseInAWard)]
    public IActionResult WithEveryDoseInAWard() => View("/Care/Round/DoseRound.cshtml", new DoseRound("Morning", DoseRoundEndpoints.DosesWithEveryDoseInAWard));

    [HttpGet(DoseRoundEndpoints.DosesWithAWardlessDose)]
    public IActionResult DosesWithAWardlessDose() => Content("""[{"ward":"North","priority":1},{"priority":3},{"ward":"South","priority":null}]""", JsonText);

    [HttpGet(DoseRoundEndpoints.DosesWithEveryDoseInAWard)]
    public IActionResult DosesWithEveryDoseInAWard() => Content("""[{"ward":"North","priority":1},{"ward":"North","priority":3},{"ward":"South","priority":null}]""", JsonText);
}

public sealed record DoseRound(string Name, string Doses);

public sealed class WardDose
{
    public string? Ward { get; init; }

    public int? Priority { get; init; }
}

public static class DoseRoundEndpoints
{
    private const string Root = CarePages.Area + "round/dose-round/";
    public const string DosesWithAWardlessDose = Root + "doses/a-wardless-dose";
    public const string DosesWithEveryDoseInAWard = Root + "doses/every-dose-in-a-ward";
}

public static class DoseRounds
{
    public static readonly AppEvent Count = AppEvent.Declare();
}
