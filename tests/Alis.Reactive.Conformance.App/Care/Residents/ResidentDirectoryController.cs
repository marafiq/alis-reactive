using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentDirectoryController : Controller
{
    [HttpGet(CarePages.ResidentDirectoryUnderReplace)]
    public IActionResult UnderReplace() => View("/Care/Residents/ResidentDirectory.cshtml", new ResidentDirectory(HandlerPolicy.Replace, TheResidents()));

    [HttpGet(CarePages.ResidentDirectoryUnderOverlap)]
    public IActionResult UnderOverlap() => View("/Care/Residents/ResidentDirectory.cshtml", new ResidentDirectory(HandlerPolicy.Overlap, TheResidents()));

    [HttpGet(CarePages.ResidentPanels)]
    public IActionResult Panels() => View("/Care/Residents/ResidentPanels.cshtml", new ResidentPanels(TheResidents()));

    [HttpGet(CareEndpoints.ResidentPanel)]
    public async Task<IActionResult> Panel(ResidentPanelQuery query)
    {
        await World.Reach(GateName.Of(LanesGate.Respond), Request.Path);
        var resident = TheResidents().Find(row => row.Id == query.ResidentId);
        return resident is null ? NotFound() : PartialView("/Care/Residents/ResidentPanel.cshtml", new ResidentPanel(resident.Id, resident.Name));
    }

    private WorldData World => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;

    private static List<ResidentRow> TheResidents() => [new ResidentRow { Id = 1, Name = "Ada" }, new ResidentRow { Id = 2, Name = "Bea" }, new ResidentRow { Id = 3, Name = "Cy" }];
}

public sealed record ResidentDirectory(HandlerPolicy Policy, IReadOnlyList<ResidentRow> Residents);

public sealed class ResidentRow
{
    public int Id { get; init; }

    public string Name { get; init; } = "";
}

public sealed class ResidentPanelQuery
{
    public int ResidentId { get; set; }
}

public sealed record ResidentPanels(IReadOnlyList<ResidentRow> Residents);

public sealed record ResidentPanel(int Id, string Name);
