using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CensusRaisedTimeController : Controller
{
    [HttpGet(CarePages.CensusRaisedTime)]
    public IActionResult OfTheVisit() => View("/Care/Residents/CensusRaisedTime.cshtml", new CensusRaisedTime());
}

public sealed class CensusRaisedTime;
