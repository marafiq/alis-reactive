using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LanguageIntakeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.LanguageIntake)]
    public IActionResult OfAResident()
        => View("/Care/AdmissionClinical/LanguageIntake.cshtml", new LanguageIntake { Languages = [new CareLanguage("Tagalog", "tl"), new CareLanguage("Punjabi", "pa")] });

    [HttpPost(LanguageIntakeEndpoints.Languages)]
    public IActionResult Save([FromForm] LanguageIntake intake) => Content($"Language: {intake.LanguageCode}", PlainText);
}

public sealed class LanguageIntake
{
    public string? LanguageCode { get; set; }

    public IReadOnlyList<CareLanguage> Languages { get; init; } = [];
}

public sealed record CareLanguage([property: JsonPropertyName("label")] string DisplayLabel, string LanguageCode);

public static class LanguageIntakeEndpoints
{
    public const string Languages = CarePages.LanguageIntake + "/languages";
}

public static class LanguageIntakes
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
