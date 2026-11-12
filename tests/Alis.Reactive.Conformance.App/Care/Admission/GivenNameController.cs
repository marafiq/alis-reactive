using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class GivenNameController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.GivenName)]
    public IActionResult OfTheNextOfKin() => View("/Care/Admission/GivenName.cshtml", new IntakeName());

    [HttpPost(GivenNameEndpoints.Names)]
    public IActionResult Save([FromBody] IntakeName name) => Content(name.GivenName ?? "(none)", PlainText);
}

public sealed class IntakeName
{
    [JsonPropertyName("given")]
    public string? GivenName { get; set; }
}

public static class GivenNameEndpoints
{
    public const string Names = CarePages.Area + "admission/given-name/names";
}

public static class GivenNames
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
