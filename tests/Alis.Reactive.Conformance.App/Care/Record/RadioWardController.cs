using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RadioWardController : Controller
{
    [HttpGet(CarePages.RadioWard)]
    public IActionResult WithItsRadio() => View("/Care/Record/RadioWard.cshtml", new RadioWard("East ward", [RecordScripts.CardRadio]));

    [HttpGet(CarePages.RadioWardWithAStuckRadio)]
    public IActionResult WithAStuckRadio() => View("/Care/Record/RadioWard.cshtml", new RadioWard("East ward", [RecordScripts.CardRadioWithAStuckUnsubscribe]));

    [HttpGet(RadioWardEndpoints.AdasCard)]
    public IActionResult AdasCard() => PartialView("/Care/Record/RadioWardAdaCard.cshtml", new RadioWardAdaCard("Ada Byron", new RadioWardAdaNotes("Turn Ada every two hours.")));

    [HttpGet(RadioWardEndpoints.BensCard)]
    public IActionResult BensCard() => PartialView("/Care/Record/RadioWardBenCard.cshtml", new RadioWardBenCard("Ben Okafor"));
}

public sealed record RadioWard(string Ward, IReadOnlyList<string> Scripts) : IScriptedRecordPage;

public sealed record RadioWardAdaCard(string Name, RadioWardAdaNotes Notes);

public sealed record RadioWardAdaNotes(string Text);

public sealed record RadioWardBenCard(string Name);

public static class RadioWardEndpoints
{
    private const string Root = CarePages.Area + "record/radio-ward/";

    public const string AdasCard = Root + "ada";

    public const string BensCard = Root + "ben";
}

public static class RadioWardButtons
{
    public static readonly AppEvent OpenAdasCard = AppEvent.Declare();

    public static readonly AppEvent OpenBensCard = AppEvent.Declare();

    public static readonly AppEvent CallTheWard = AppEvent.Declare();

    public static readonly AppEvent PutAdasNotesAway = AppEvent.Declare();

    public static readonly AppEvent CountForBen = AppEvent.Declare();
}
