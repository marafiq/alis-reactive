using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AllergySaveController : Controller
{
    private const string Page = "/Care/AdmissionClinical/AllergySave.cshtml";

    [HttpGet(CarePages.AllergySaveOfTwo)]
    public IActionResult OfTwo() => View(Page, new AllergySave { AllergyCodes = [7, 9] });

    [HttpGet(CarePages.AllergySaveOfNone)]
    public IActionResult OfNone() => View(Page, new AllergySave());

    [HttpPost(AllergySaveEndpoints.Allergies)]
    public IActionResult Saved([FromBody] AllergyUpdate update)
        => Json(new SavedAllergies(update.AllergyCodes is { Length: > 0 } codes ? $"Saved {string.Join(", ", codes)}" : "Saved none"));
}

public sealed class AllergySave
{
    public int[]? AllergyCodes { get; set; }
}

public sealed class AllergyUpdate
{
    public int[]? AllergyCodes { get; set; }
}

public sealed record SavedAllergies(string Message);

public static class AllergySaveEndpoints
{
    public const string Allergies = CarePages.AllergySaveOfTwo + "/allergies";
}

public static class AllergySaves
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
