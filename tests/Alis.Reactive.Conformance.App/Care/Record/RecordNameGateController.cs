using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RecordNameGateController : Controller
{
    [HttpGet(CarePages.RecordNameGate)]
    public IActionResult OfAResident() => View("/Care/Record/RecordNameGate.cshtml", new RecordNameGate("Ada Hart"));
}

public sealed record RecordNameGate(string Resident);
