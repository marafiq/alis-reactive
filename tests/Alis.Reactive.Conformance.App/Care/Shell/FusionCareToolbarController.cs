using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FusionCareToolbarController : Controller
{
    [HttpGet(CarePages.FusionCareToolbar)]
    public IActionResult Workflows() => View("/Care/Shell/FusionCareToolbar.cshtml", new FusionCareToolbar("Admit", "Discharge"));
}

public sealed record FusionCareToolbar(string AdmitLabel, string DischargeLabel);
