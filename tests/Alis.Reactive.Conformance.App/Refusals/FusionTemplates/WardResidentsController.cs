using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardResidentsController : Controller
{
    [HttpGet(RefusalPages.WardResidentsByAComputedWard)]
    public IActionResult ByAComputedWard() => View("/Refusals/FusionTemplates/WardResidents.cshtml", new WardResidents("north", [new WardResident("Ada", "NORTH"), new WardResident("Ruth", "SOUTH")]));

    [HttpGet(RefusalPages.WardResidentsByALocalWard)]
    public IActionResult ByALocalWard() => View("/Refusals/FusionTemplates/WardResidentsByALocal.cshtml", new WardResidents("north", [new WardResident("Ada", "NORTH"), new WardResident("Ruth", "SOUTH")]));
}

public sealed record WardResidents(string Ward, IReadOnlyList<WardResident> Residents);

public sealed record WardResident(string Name, string Ward);
