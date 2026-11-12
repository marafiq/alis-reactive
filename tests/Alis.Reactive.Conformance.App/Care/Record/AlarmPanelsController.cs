using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AlarmPanelsController : Controller
{
    [HttpGet(CarePages.AlarmPanels)]
    public IActionResult OfAResident() => View("/Care/Record/AlarmPanels.cshtml", new AlarmPanels("Ada Hart", "Gamma"));
}

public sealed record AlarmPanels(string Resident, string Outer);
