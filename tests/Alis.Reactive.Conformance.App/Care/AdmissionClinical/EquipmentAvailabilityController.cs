using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class EquipmentAvailabilityController : Controller
{
    [HttpGet(CarePages.EquipmentAvailability)]
    public IActionResult Desk() => View("/Care/AdmissionClinical/EquipmentAvailability.cshtml", new EquipmentAvailability());

    [HttpPost(CareEndpoints.TransportStock)]
    public async Task<IActionResult> Stock([FromBody] TransportStockQuery query)
    {
        await HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(GateName.Of(RunGate.Settle), Request.Path);
        TransportStockRow[] rows = query.Aid switch
        {
            "chair" => [new(false, new("Chair W-09 is being cleaned")), new(true, new("Collect chair W-14 from North stores"))],
            "frame" => [new(true, new("Collect frame F-08 from South stores"))],
            _ => [],
        };
        return Json(rows);
    }
}

public sealed class EquipmentAvailability
{
    public string? TransportAid { get; set; }

    public IReadOnlyList<TransportAidChoice> Choices { get; } = [new("chair", "Wheelchair"), new("frame", "Walking frame")];
}

public sealed record TransportAidChoice(string Code, string Name);

public sealed record TransportStockQuery(string Aid);

public sealed record TransportStockRow(bool ReadyForCollection, TransportDispatch Dispatch);

public sealed record TransportDispatch(string Instruction);
