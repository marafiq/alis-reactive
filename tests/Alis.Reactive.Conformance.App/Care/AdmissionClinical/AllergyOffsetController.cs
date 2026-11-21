using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyOffsetController : Controller
{
    private const string Page = "/Care/AdmissionClinical/AllergyOffset.cshtml";

    [HttpGet(CarePages.AllergyOffsetWritten)]
    public IActionResult Written() => View(Page, new AllergyOffset { MovesTheList = true });

    [HttpGet(CarePages.AllergyOffsetRead)]
    public IActionResult Read() => View(Page, new AllergyOffset());
}

public sealed class AllergyOffset
{
    public int[]? AllergyCodes { get; set; }

    public bool MovesTheList { get; init; }
}
