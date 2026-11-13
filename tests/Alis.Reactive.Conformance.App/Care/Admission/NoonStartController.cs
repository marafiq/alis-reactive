using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NoonStartController : Controller
{
    [HttpGet(CarePages.NoonStart)]
    public IActionResult OfTheRound(NoonStart start) => View("/Care/Admission/NoonStart.cshtml", start);
}

public sealed class NoonStart
{
    public TimeOnly? Starts { get; set; }
}
