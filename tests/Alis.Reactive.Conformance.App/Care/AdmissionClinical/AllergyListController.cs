using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyListController : Controller
{
    [HttpGet(CarePages.AllergyList)]
    public IActionResult OfHelensWard() => View("/Care/AdmissionClinical/AllergyList.cshtml", new AllergyList());
}

public sealed class AllergyList
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class AllergyListWard
{
    public string Ward { get; init; } = "";
}

public static class AllergyLists
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
