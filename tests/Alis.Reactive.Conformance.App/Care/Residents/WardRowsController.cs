using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardRowsController : Controller
{
    private const string ScriptedLayout = "/Care/Residents/_ScriptedLayout.cshtml";
    private const string SharedLayout = "/Shared/_Layout.cshtml";

    [HttpGet(CarePages.ResidentDischarges)]
    public IActionResult Discharges() => View("/Care/Residents/ResidentDischarges.cshtml", new ResidentDischarges(TheResidents()));

    [HttpGet(CarePages.BusyDischarges)]
    public IActionResult BusyDischarges() => View("/Care/Residents/BusyDischarges.cshtml", new BusyDischarges(TheResidents()));

    [HttpPost(CareEndpoints.ResidentDischarge)]
    public async Task<IActionResult> Discharge(int residentId)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        var resident = TheResidents().Find(row => row.Id == residentId);
        return resident is null ? NotFound() : Json(new DischargeAnswer(resident.Name + " discharged"));
    }

    [HttpGet(CarePages.ResidentLinks)]
    public IActionResult Links() => View("/Care/Residents/ResidentLinks.cshtml", new ResidentLinks(TheResidents()));

    [HttpGet(CarePages.ResidentLinksBehindAClickGuard)]
    public IActionResult LinksBehindAClickGuard()
        => View("/Care/Residents/GuardedResidentLinks.cshtml", new GuardedResidentLinks(ScriptedLayout, [ResidentsScripts.EveryClickPrevented], TheResidents()));

    [HttpGet(CarePages.ResidentLinksWithNoClickGuard)]
    public IActionResult LinksWithNoClickGuard() => View("/Care/Residents/GuardedResidentLinks.cshtml", new GuardedResidentLinks(SharedLayout, [], TheResidents()));

    [HttpGet(CarePages.ResidentKeys)]
    public IActionResult Keys()
        => View("/Care/Residents/ResidentKeys.cshtml", new ResidentKeys([new("Ada Lovelace"), new("a+b"), new("O'Neil"), new("Unit (2)"), new("~north"), new("Zoé")]));

    [HttpGet(CarePages.ResidentKeysOfLetters)]
    public IActionResult KeysOfLetters() => View("/Care/Residents/ResidentKeys.cshtml", new ResidentKeys([new("Ada")]));

    [HttpGet(WardRowsEndpoints.ResidentByKey)]
    public IActionResult ByKey(string key) => Json(new KeyReceived(key));

    [HttpGet(CarePages.DischargeList)]
    public IActionResult DischargeList() => View("/Care/Residents/DischargeList.cshtml", new DischargeList());

    [HttpGet(WardRowsEndpoints.DischargeListRows)]
    public IActionResult DischargeListRows() => PartialView("/Care/Residents/DischargeListRows.cshtml", new DischargeListRows(TheResidents()));

    [HttpPost(CareEndpoints.CountedDischarge)]
    public IActionResult CountedDischarge(int residentId)
        => TheResidents().Exists(row => row.Id == residentId) ? Json(new DischargeCount(World.Discharged())) : NotFound();

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private static List<ResidentRow> TheResidents() => [new ResidentRow { Id = 1, Name = "Ada" }, new ResidentRow { Id = 2, Name = "Bea" }, new ResidentRow { Id = 3, Name = "Cy" }];
}

public static class WardRowsEndpoints
{
    public const string ResidentByKey = "/Residents/{key}";

    public const string DischargeListRows = "/WardRows/discharge-list";
}

public sealed record ResidentDischarges(IReadOnlyList<ResidentRow> Residents);

public sealed record BusyDischarges(IReadOnlyList<ResidentRow> Residents);

public sealed class DischargeRequest
{
    public int ResidentId { get; set; }
}

public sealed record DischargeAnswer(string Text);

public sealed record ResidentLinks(IReadOnlyList<ResidentRow> Residents);

public sealed record GuardedResidentLinks(string Layout, IReadOnlyList<string> Scripts, IReadOnlyList<ResidentRow> Residents) : IScriptedResidentsPage;

public sealed record ResidentKeys(IReadOnlyList<ResidentKeyRow> Rows);

public sealed record ResidentKeyRow(string Key);

public sealed class ResidentKeyQuery
{
    public string Key { get; set; } = "";
}

public sealed record KeyReceived(string Key);

public sealed record DischargeList;

public sealed record DischargeListRows(IReadOnlyList<ResidentRow> Residents);

public sealed record DischargeCount(int Count);

public static class DischargeListButtons
{
    public static readonly AppEvent LoadResidents = AppEvent.Declare();
}
