using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class OvernightStayController : Controller
{
    [HttpGet(CarePages.OvernightStay)]
    public IActionResult OfAResident() => View("/Care/Admission/OvernightStay.cshtml", new OvernightStay());

    [HttpGet(OvernightStayEndpoints.Overnight)]
    public IActionResult Overnight() => PartialView("/Care/Admission/OvernightStaySection.cshtml", new OvernightStay());
}

public sealed class OvernightStay
{
    public string? Resident { get; set; }

    public int Nights { get; set; } = 14;
}

public static class OvernightStayEndpoints
{
    public const string Overnight = CarePages.Area + "admission/overnight-stay/overnight";
}

public static class OvernightStays
{
    public static readonly AppEvent AddTheOvernight = AppEvent.Declare();

    public static readonly AppEvent RemoveTheOvernight = AppEvent.Declare();

    public static readonly AppEvent Quote = AppEvent.Declare();
}
