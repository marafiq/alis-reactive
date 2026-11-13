using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedsideRecordController : Controller
{
    [HttpGet(CarePages.BedsideRecord)]
    public IActionResult ForAResident() => View("/Care/Record/BedsideRecord.cshtml", new BedsideRecord("Ada Hart"));
}

public sealed record BedsideRecord(string Resident);
