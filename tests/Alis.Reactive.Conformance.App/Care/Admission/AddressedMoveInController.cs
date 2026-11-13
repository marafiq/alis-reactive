using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AddressedMoveInController : Controller
{
    private const string PlainText = "text/plain";
    private const string NoSuchStreet = "Unknown";
    private const string StreetUnknown = "Give a street the post office knows.";

    [HttpGet(CarePages.AddressedMoveIn)]
    public IActionResult OfAResident() => View("/Care/Admission/AddressedMoveIn.cshtml", new AddressedMoveIn());

    [HttpPost(AddressedMoveInEndpoints.MoveIns)]
    public IActionResult Save([FromForm] AddressedMoveIn moveIn)
    {
        if (moveIn.Home.Street == NoSuchStreet) ModelState.AddModelError("Home.Street", StreetUnknown);
        if (moveIn.Postal.Street == NoSuchStreet) ModelState.AddModelError("Postal.Street", StreetUnknown);
        if (ModelState.IsValid) return Content("Saved", PlainText);
        return ValidationProblem(ModelState);
    }
}

public sealed class AddressedMoveIn
{
    public string? Name { get; set; }

    public MoveInAddress Home { get; set; } = new();

    public MoveInAddress Postal { get; set; } = new();
}

public sealed class MoveInAddress
{
    public string? Street { get; set; }
}

public static class AddressedMoveInEndpoints
{
    public const string MoveIns = CarePages.AddressedMoveIn + "/move-ins";
}

public static class AddressedMoveIns
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
