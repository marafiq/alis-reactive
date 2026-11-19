using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Fusion.FusionComponents;
using Alis.Reactive.Plugins;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SupplierDeliveryController : Controller
{
    private static readonly IReadOnlyList<SupplierSchedule> Suppliers = [
        new(new("northbank", "Northbank Medical"), [new(false, new("Monday's first van is fully booked")), new(true, new("Northbank delivers Monday at 09:30"))]),
        new(new("harbor", "Harbor Mobility"), [new(true, new("Harbor delivers Tuesday at 14:00"))]),
    ];

    [HttpGet(CarePages.SupplierDelivery)]
    public IActionResult Desk() => View("/Care/AdmissionClinical/SupplierDelivery.cshtml", new SupplierDelivery());

    [HttpPost(CareEndpoints.SupplierCatalogue)]
    public async Task<IActionResult> Catalogue([FromBody] SupplierCatalogueQuery query)
    {
        var world = HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data;
        if (query.Supplier is null)
        {
            var options = Suppliers.Select(supplier => supplier.Option).ToArray();
            return query.RequiresCounts ? Json(new { result = options, count = options.Length }) : Json(options);
        }

        await world.Reach(GateName.Of(RunGate.Settle), Request.Path);
        return Json(Suppliers.FirstOrDefault(supplier => supplier.Option.Code == query.Supplier)?.Windows ?? []);
    }

    private sealed record SupplierSchedule(SupplierOption Option, IReadOnlyList<SupplierWindow> Windows);
}

public sealed class SupplierDelivery
{
    public string? Supplier { get; set; }
}

public sealed record SupplierOption(string Code, string Name);

public sealed record SupplierCatalogueQuery(string? Supplier, bool RequiresCounts);

public sealed record SupplierWindow(bool Bookable, SupplierDispatch Dispatch);

public sealed record SupplierDispatch(string Instruction);

[Mirrors("Care/AdmissionClinical/supplier-queries", "SupplierQueries")]
public sealed class SupplierQueries : Plugin
{
    public SupplierQueries() : base("supplier-queries")
    {
    }

    public ReadOnly<FusionComboBox<string?>.DataQuery> Availability => ReadOnly<FusionComboBox<string?>.DataQuery>();
}
