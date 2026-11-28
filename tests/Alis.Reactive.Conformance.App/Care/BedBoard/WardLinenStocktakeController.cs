using System.Text.Json;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class WardLinenStocktakeController : Controller
{
    private const string Stocktake = "/Care/BedBoard/WardLinenStocktake.cshtml";

    [HttpGet(CarePages.LinenStocktake)]
    public IActionResult Floor() => View(Stocktake, new WardLinenStocktake(8));

    [HttpGet(CarePages.LinenReceiving)]
    public IActionResult Receiving() => View(Stocktake, new WardLinenStocktake(8, Receiving: true));

    [HttpGet(CarePages.LinenKept)]
    public IActionResult Recovered() => View(Stocktake, new WardLinenStocktake(8, Recover: true));

    [HttpPost(CarePages.LinenFileStocktake)]
    public IActionResult FileCount([FromForm] WardStockReturn stock) => Content(stock.Towels is { } count
        ? $"Ward stocktake filed with {count} towels"
        : "Ward stocktake filed awaiting a count");

    [HttpPost(CarePages.LinenArchiveStocktake)]
    public IActionResult Archive([FromBody] WardStockArchive archive)
    {
        var snapshot = JsonSerializer.Deserialize<ArchivedWardCount>(archive.Snapshot, JsonSerializerOptions.Web);
        return Content(snapshot?.Value switch
        {
            null => "Archived outstanding count",
            0 => "Archived empty cart",
            var count => $"Archived stock of {count} towels",
        });
    }

    [HttpGet(CarePages.LinenLaundryCharge)]
    public IActionResult Charge() => View("/Care/BedBoard/LinenLaundryCharge.cshtml", new LaundryInvoice(12.5m));

    [HttpGet(CarePages.LinenScale)]
    public IActionResult Weigh() => View("/Care/BedBoard/LinenScale.cshtml", new LinenBundleWeighing(12.375m));

    [HttpGet(CarePages.LinenScaleSheet)]
    public IActionResult PrintSlip() => View("/Care/BedBoard/LinenScaleSheet.cshtml", new LaundryWeighingSlip(null));

    [HttpGet(CarePages.LinenRecordedWeight)]
    public IActionResult Recorded() => View("/Care/BedBoard/LinenWeightReturn.cshtml", new LinenWeightReturn(12.5m));

    [HttpGet(CarePages.LinenUnrecordedWeight)]
    public IActionResult Unrecorded() => View("/Care/BedBoard/LinenWeightReturn.cshtml", new LinenWeightReturn(null));

    [HttpGet(CarePages.LinenTransfer)]
    public IActionResult Transfer() => View("/Care/BedBoard/LinenTransfer.cshtml", new LinenTransfer(8));

    [HttpGet(CarePages.LinenParcel)]
    public IActionResult Parcel() => View("/Care/BedBoard/LinenParcel.cshtml", new LinenParcel(12.5m, 0.25d));

    [HttpGet(CarePages.LinenEmergency)]
    public IActionResult Emergency() => View("/Care/BedBoard/EmergencyLinenStocktake.cshtml", new EmergencyLinenStocktake(8));

    [HttpGet(CarePages.LinenStaffedStation)]
    public IActionResult Staffed() => View("/Care/BedBoard/LinenStockStation.cshtml", new LinenStockStation(8, true));

    [HttpGet(CarePages.LinenArchivedStation)]
    public IActionResult Archived() => View("/Care/BedBoard/LinenStockStation.cshtml", new LinenStockStation(8, false));

    [HttpGet(CarePages.LinenEuropeanTariff)]
    public IActionResult European() => View("/Care/BedBoard/LinenSupplierTariff.cshtml", new LinenSupplierTariff(12.375m, true));

    [HttpGet(CarePages.LinenDomesticTariff)]
    public IActionResult Domestic() => View("/Care/BedBoard/LinenSupplierTariff.cshtml", new LinenSupplierTariff(12.375m, false));

    [HttpPost(CarePages.LinenSignedReturn)]
    public IActionResult Sign([FromForm] WardStockReturn stock) => Request.Form.ContainsKey(nameof(WardStockReturn.Towels))
        ? View("/Care/BedBoard/LinenSignedReceipt.cshtml", stock)
        : BadRequest("The signed return does not contain the stocktake field.");
}

public sealed record WardLinenStocktake(int? Towels, bool Receiving = false, bool Recover = false);

public sealed record WardStockReturn(int? Towels);

public sealed record WardStockArchive(string Snapshot);

public sealed record ArchivedWardCount(int? Value);

public sealed record LaundryInvoice(decimal? Charge);

public sealed record LinenBundleWeighing(decimal? Kilograms);

public sealed record LaundryWeighingSlip(decimal? Reading);

public sealed record LinenWeightReturn(decimal? Weight);

public sealed record LinenTransfer(double? Towels);

public sealed record LinenParcel(decimal? Load, double ParcelWeight);

public sealed record EmergencyLinenStocktake(int? Towels);

public sealed class EmergencyStocktakeRules : ReactiveValidator<EmergencyLinenStocktake>
{
    public EmergencyStocktakeRules() => RuleFor(stock => stock.Towels)
        .GreaterThanOrEqualTo(8).WithMessage("Keep eight towels for emergency stock");
}

public sealed record LinenStockStation(int? Towels, bool Staffed);

public sealed record LinenSupplierTariff(decimal? Rate, bool European);
