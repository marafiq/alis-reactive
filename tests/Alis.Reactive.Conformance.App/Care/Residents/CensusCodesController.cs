using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusCodesController : Controller
{
    [HttpGet(CarePages.CensusCodesWithACodeLeftOut)]
    public IActionResult ACodeLeftOut() => View("/Care/Residents/CensusCodes.cshtml", new CensusCodes(
    [
        new RecordStart { Name = "Martin", Code = RecordCode.Text("C0") },
        new RecordStart { Name = "Adams", Code = RecordCode.Text("C1") },
        new RecordStart { Name = "Lee" },
    ]));

    [HttpGet(CarePages.CensusCodesWithACodeSentNull)]
    public IActionResult ACodeSentNull() => View("/Care/Residents/CensusCodes.cshtml", new CensusCodes(
    [
        new RecordStart { Name = "Martin", Code = RecordCode.Text("C0") },
        new RecordStart { Name = "Adams", Code = RecordCode.Text("C1") },
        new RecordStart { Name = "Lee", Code = RecordCode.Null },
    ]));
}

public sealed record CensusCodes(IReadOnlyList<RecordStart> Records);
