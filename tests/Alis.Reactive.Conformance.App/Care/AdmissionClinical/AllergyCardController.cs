using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyCardController : Controller
{
    [HttpGet(CarePages.AllergyCard)]
    public IActionResult Index() => View("/Care/AdmissionClinical/AllergyCardPage.cshtml", new AllergyCardPage());

    [HttpGet(AllergyCardEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/AdmissionClinical/AllergyCard.cshtml", new AllergyCard { AllergyCodes = [7] });
}

public sealed class AllergyCardPage
{
    public string Resident { get; set; } = "Helen Martinez";
}

public sealed class AllergyCard
{
    public int[]? AllergyCodes { get; set; }
}

public static class AllergyCardEndpoints
{
    public const string Card = CarePages.AllergyCard + "/card";
}

public static class AllergyCards
{
    public static readonly AppEvent Open = AppEvent.Declare();
}
