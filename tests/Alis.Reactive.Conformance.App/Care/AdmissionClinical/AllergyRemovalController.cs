using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyRemovalController : Controller
{
    [HttpGet(CarePages.AllergyRemoval)]
    public IActionResult Index() => View("/Care/AdmissionClinical/AllergyRemoval.cshtml", new AllergyRemoval());

    [HttpGet(AllergyRemovalEndpoints.Card)]
    public IActionResult Card() => PartialView("/Care/AdmissionClinical/AllergyRemovalCard.cshtml", new AllergyRemovalCard { AllergyCodes = [7] });
}

public sealed class AllergyRemoval
{
    public string Resident { get; set; } = "Helen Martinez";
}

public sealed class AllergyRemovalCard
{
    public int[]? AllergyCodes { get; set; }
}

public static class AllergyRemovalEndpoints
{
    public const string Card = CarePages.AllergyRemoval + "/card";
}

public static class AllergyRemovalCards
{
    public static readonly AppEvent Close = AppEvent.Declare();

    public static readonly AppEvent Open = AppEvent.Declare();

    public static readonly AppEvent Remove = AppEvent.Declare();
}
