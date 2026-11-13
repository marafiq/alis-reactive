using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DoseRecordController : Controller
{
    private const string Record = "/Care/Handover/DoseRecord.cshtml";

    [HttpGet(CarePages.DoseRecordGiven)]
    public IActionResult Given() => View(Record, new DoseRecord { DoseMg = 12.5m });

    [HttpGet(CarePages.DoseRecordBlank)]
    public IActionResult Blank() => View(Record, new DoseRecord());
}

public sealed class DoseRecord
{
    public decimal? DoseMg { get; set; }
}
