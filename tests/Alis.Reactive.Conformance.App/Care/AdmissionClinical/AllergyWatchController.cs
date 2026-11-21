using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyWatchController : Controller
{
    [HttpGet(CarePages.AllergyWatch)]
    public IActionResult Index() => View("/Care/AdmissionClinical/AllergyWatch.cshtml", new AllergyWatch());

    [HttpGet(AllergyWatchEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/AdmissionClinical/AllergyWatchCard.cshtml", new AllergyWatchCard("The card watches Helen's allergies."));
}

public sealed class AllergyWatch
{
    public int[]? AllergyCodes { get; set; }
}

public sealed record AllergyWatchCard(string Note);

public static class AllergyWatchEndpoints
{
    public const string Card = CarePages.AllergyWatch + "/card";
}

public static class AllergyWatchCards
{
    public static readonly AppEvent Close = AppEvent.Declare();
}
