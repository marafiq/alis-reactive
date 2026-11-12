using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class SignedVisitController : Controller
{
    [HttpGet(RefusalPages.SignedVisitNamedInPascalCase)]
    public IActionResult NamedInPascalCase() => View("/Refusals/TriggersAndPipelines/SignedVisitNamedInPascalCase.cshtml", new SignedVisit("Ada Hart"));

    [HttpGet(RefusalPages.SignedVisitNamedInCamelCase)]
    public IActionResult NamedInCamelCase() => View("/Refusals/TriggersAndPipelines/SignedVisitNamedInCamelCase.cshtml", new SignedVisit("Ada Hart"));
}

public sealed record SignedVisit(string Visitor);

public sealed class PascalNamedSignature
{
    [JsonPropertyName("VisitorName")]
    public string Visitor { get; init; } = "";
}

public sealed class CamelNamedSignature
{
    [JsonPropertyName("visitorName")]
    public string Visitor { get; init; } = "";
}

public static class PascalNamedVisits
{
    public static readonly AppEvent<PascalNamedSignature> Signed = AppEvent.Declare<PascalNamedSignature>();
}

public static class CamelNamedVisits
{
    public static readonly AppEvent<CamelNamedSignature> Signed = AppEvent.Declare<CamelNamedSignature>();
}
