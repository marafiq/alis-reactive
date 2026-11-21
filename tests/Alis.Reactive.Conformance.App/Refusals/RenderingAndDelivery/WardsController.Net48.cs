using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class WardsController : Controller
{
    private const string WardsView = "/Refusals/RenderingAndDelivery/Wards.Net48.cshtml";
    private const string WardView = "/Refusals/RenderingAndDelivery/Ward.Net48.cshtml";
    private const string MisspelledRuntimeLayout = "/Refusals/RenderingAndDelivery/_MisspelledRuntimeLayout.Net48.cshtml";
    private const string TheTwinsLayout = "/Shared/_Layout.cshtml";

    [HttpGet]
    [Address(RefusalPages.WardsLinkingAMisspelledRuntime)]
    public ActionResult LinkingAMisspelledRuntime() => View(WardsView, new WardsIndex(WardsEndpoints.WardLinkingAMisspelledRuntime));

    [HttpGet]
    [Address(RefusalPages.WardsLinkingTheirRuntime)]
    public ActionResult LinkingTheirRuntime() => View(WardsView, new WardsIndex(WardsEndpoints.WardLinkingItsRuntime));

    [HttpGet]
    [Address(WardsEndpoints.WardLinkingAMisspelledRuntime)]
    public ActionResult WardLinkingAMisspelledRuntime() => View(WardView, MisspelledRuntimeLayout, new WardPage("North"));

    [HttpGet]
    [Address(WardsEndpoints.WardLinkingItsRuntime)]
    public ActionResult WardLinkingItsRuntime() => View(WardView, TheTwinsLayout, new WardPage("North"));
}

public sealed record WardsIndex(string Ward);

public sealed record WardPage(string Name);

public static class WardsEndpoints
{
    public const string WardLinkingAMisspelledRuntime = RefusalPages.WardsLinkingAMisspelledRuntime + "/ward";
    public const string WardLinkingItsRuntime = RefusalPages.WardsLinkingTheirRuntime + "/ward";
}
