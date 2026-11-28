using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusSortController : Controller
{
    [HttpGet(CarePages.CensusSortedByName)]
    public IActionResult ByName() => View("/Care/Residents/CensusSort.cshtml", new CensusSort(Descending: false, InOrderOfAdmission()));

    [HttpGet(CarePages.CensusSortedByNameDescending)]
    public IActionResult ByNameDescending() => View("/Care/Residents/CensusSort.cshtml", new CensusSort(Descending: true, InOrderOfAdmission()));

    private static IReadOnlyList<RecordStart> InOrderOfAdmission()
        => [new RecordStart { Name = "Martin" }, new RecordStart { Name = "Adams" }, new RecordStart { Name = "Lee" }];
}

public sealed record CensusSort(bool Descending, IReadOnlyList<RecordStart> Admissions);

public static class CensusSorts
{
    public static readonly AppEvent Sort = AppEvent.Declare();
}
