using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StayExtensionController : Controller
{
    [HttpGet(CarePages.StayExtension)]
    public IActionResult OfNights() => View("/Care/Admission/StayExtension.cshtml", new StayExtension { Section = StayExtensionEndpoints.NightsSection });

    [HttpGet(CarePages.StayExtensionOfExtraNights)]
    public IActionResult OfExtraNights() => View("/Care/Admission/StayExtension.cshtml", new StayExtension { Section = StayExtensionEndpoints.ExtraNightsSection });

    [HttpGet(StayExtensionEndpoints.NightsSection)]
    public IActionResult NightsSection() => PartialView("/Care/Admission/StayExtensionNights.cshtml", new StayExtensionSection());

    [HttpGet(StayExtensionEndpoints.ExtraNightsSection)]
    public IActionResult ExtraNightsSection() => PartialView("/Care/Admission/StayExtensionExtraNights.cshtml", new StayExtensionSection());
}

public sealed class StayExtension
{
    public int Nights { get; set; }

    public required string Section { get; init; }
}

public sealed class StayExtensionSection
{
    public int Nights { get; set; }

    public int ExtraNights { get; set; }
}

public static class StayExtensionEndpoints
{
    public const string NightsSection = CarePages.Area + "admission/stay-extension/sections/nights";

    public const string ExtraNightsSection = CarePages.Area + "admission/stay-extension/sections/extra-nights";
}

public static class StayExtensions
{
    public static readonly AppEvent AddTheExtension = AppEvent.Declare();

    public static readonly AppEvent Quote = AppEvent.Declare();
}
