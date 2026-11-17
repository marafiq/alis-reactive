using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergySearchController : Controller
{
    [HttpGet(CarePages.AllergySearch)]
    public IActionResult Index() => View("/Care/AdmissionClinical/AllergySearch.cshtml", new AllergySearch());

    [HttpGet(AllergySearchEndpoints.Allergies)]
    public IActionResult Allergies([FromQuery] AllergyQuery query)
        => Json(HelensAllergies.Offered().Where(allergy => allergy.Name.StartsWith(query.Text ?? "", StringComparison.OrdinalIgnoreCase)).ToList());
}

public sealed class AllergySearch
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class AllergyQuery
{
    public string? Text { get; set; }
}

public static class AllergySearchEndpoints
{
    public const string Allergies = CarePages.AllergySearch + "/allergies";
}
