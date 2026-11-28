using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyFieldController : Controller
{
    [HttpGet(CarePages.AllergyField)]
    public IActionResult OfHelensIntake() => View("/Care/AdmissionClinical/AllergyField.cshtml", new AllergyField());

    [HttpGet(CarePages.FieldChange)]
    public IActionResult ChangedOnHelensIntake() => View("/Care/AdmissionClinical/FieldChange.cshtml", new FieldChange());
}

public sealed class AllergyField
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class FieldChange
{
    public int[]? AllergyCodes { get; set; }
}

public static class AllergyFields
{
    public static readonly AppEvent Show = AppEvent.Declare();

    public static readonly AppEvent UseLabels = AppEvent.Declare();
}
