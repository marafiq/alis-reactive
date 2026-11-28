using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FamilyDischargeController : Controller
{
    [HttpGet(CarePages.FamilyDischarge)]
    public IActionResult OfAResident() => View("/Care/Shell/FamilyDischarge.cshtml", new FamilyDischarge("Ada"));

    [HttpGet(FamilyDischargeEndpoints.Panel)]
    public IActionResult Panel() => PartialView("/Care/Shell/FamilyDischargePanel.cshtml", new FamilyDischargePanel("Grace"));

    [HttpGet(CareEndpoints.FamilyRefresh)]
    [AtTheAnswerGate]
    public IActionResult Refresh() => PartialView("/Care/Shell/FamilyDischargeRefreshed.cshtml", new FamilyDetailsRefreshed("Family details refreshed."));
}

public sealed record FamilyDischarge(string Resident);

public sealed record FamilyDischargePanel(string Contact);

public sealed record FamilyDetailsRefreshed(string Note);

public static class FamilyDischargeEndpoints
{
    public const string Panel = CarePages.Area + "shell/family-discharge/panel";
}

public static class FamilyDischargeButtons
{
    public static readonly AppEvent RefreshFamily = AppEvent.Declare();

    public static readonly AppEvent Discharge = AppEvent.Declare();
}
