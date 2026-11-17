using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergyLabelsController : Controller
{
    [HttpGet(CarePages.AllergyLabels)]
    public IActionResult AsText() => View("/Care/AdmissionClinical/AllergyLabels.cshtml", new AllergyLabels(Offered));

    [HttpGet(CarePages.AllergyLabelsReadRaw)]
    public IActionResult ReadRaw() => View("/Care/AdmissionClinical/AllergyLabelsReadRaw.cshtml", new AllergyLabels(Offered));

    private static IReadOnlyList<LabelledAllergy> Offered => [new("Penicillin", 7), new("<i>Latex</i>", 9)];
}

public sealed class AllergyLabels(IReadOnlyList<LabelledAllergy> offered)
{
    public IReadOnlyList<LabelledAllergy> Offered { get; } = offered;

    public int[]? AllergyCodes { get; set; }
}

public sealed record LabelledAllergy(string Name, int Code);
