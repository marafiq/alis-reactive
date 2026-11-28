using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CareSectionsController : Controller
{
    private const string Sections = "/Care/RecordContacts/CareSections.cshtml";

    [HttpGet(CarePages.CareSectionsAtAllergies)]
    public IActionResult AtAllergies() => View(Sections, new CareSections { OpenSection = 1 });

    [HttpGet(CarePages.CareSectionsAtMedications)]
    public IActionResult AtMedications() => View(Sections, new CareSections { OpenSection = 0 });
}

public sealed class CareSections
{
    public double OpenSection { get; init; }
}

public static class CareSectionOpenings
{
    public static readonly AppEvent Diet = AppEvent.Declare();
}
