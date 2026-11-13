using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentPreviewController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ResidentPreview)]
    public IActionResult AsTheAppWritesIt() => View("/Care/AdmissionClinical/ResidentPreview.cshtml", new ResidentPreview { Address = ResidentPreviewEndpoints.AsWritten, GivenName = "Rose" });

    [HttpGet(CarePages.ResidentPreviewInCamelCase)]
    public IActionResult InCamelCase() => View("/Care/AdmissionClinical/ResidentPreview.cshtml", new ResidentPreview { Address = ResidentPreviewEndpoints.InCamelCase, GivenName = "Rose" });

    [HttpGet(ResidentPreviewEndpoints.AsWritten)]
    public IActionResult AsWritten() => Content(JsonConvert.SerializeObject(new PreviewedResident(7, "Rose")), PlainText);

    [HttpGet(ResidentPreviewEndpoints.InCamelCase)]
    public IActionResult CamelCased()
        => Content(JsonConvert.SerializeObject(new PreviewedResident(7, "Rose"), new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }), PlainText);
}

public sealed class ResidentPreview
{
    public string Address { get; init; } = null!;

    public string? GivenName { get; set; }
}

public sealed record PreviewedResident(int ResidentId, string GivenName);

public static class ResidentPreviewEndpoints
{
    public const string AsWritten = CarePages.ResidentPreview + "/resident";

    public const string InCamelCase = CarePages.ResidentPreviewInCamelCase + "/resident";
}

public static class ResidentPreviews
{
    public static readonly AppEvent Preview = AppEvent.Declare();
}
