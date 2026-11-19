using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CommunicationPreferencesController : Controller
{
    private CommunicationPlanFile FileOfHelen => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(CommunicationPlanFile.Open);

    [HttpGet(CarePages.CommunicationPreferences)]
    public IActionResult OfHelen() => View("/Care/AdmissionClinical/CommunicationPreferences.cshtml", FileOfHelen.Current);

    [HttpPost(CommunicationPreferences.SaveEndpoint)]
    public IActionResult Save([FromBody] CommunicationPlan plan)
    {
        if (!FileOfHelen.Save(plan)) return BadRequest();

        return Content("Communication plan saved for Helen Martinez.", "text/plain");
    }
}

internal sealed class CommunicationPlanFile : IOwnedByAWorld
{
    private volatile CommunicationPreferences _current = new();

    internal CommunicationPreferences Current => _current;

    internal static CommunicationPlanFile Open() => new();

    void IOwnedByAWorld.Take(Seed seed) { }

    internal bool Save(CommunicationPlan plan)
    {
        var language = CommunicationPreferences.Languages.SingleOrDefault(row => row.Code == plan.LanguageCode);
        if (language is null || language.OptionLabel != plan.SelectedLabel || language.Support.Spoken.BookingTeam != plan.BookingTeam
            || language.Support.Spoken.RequiresInterpreter != plan.RequiresInterpreter || language.Support.Written.NeedsTranslatedPack != plan.NeedsTranslatedPack
            || !CommunicationPreferences.Languages.Any(row => row.Label == plan.PreviousLanguage)
            || plan.Channel is not ("telephone" or "letter") || plan.LeadMinutes is not (0 or 30) || plan.Consent is null)
        {
            return false;
        }

        _current = new CommunicationPreferences
        {
            Channel = plan.Channel,
            LeadMinutes = plan.LeadMinutes,
            Consent = plan.Consent,
            Selected = language,
            PreviousLanguage = plan.PreviousLanguage!,
            Saved = true,
        };
        return true;
    }
}

public sealed class CommunicationPreferences
{
    public const string SaveEndpoint = CarePages.CommunicationPreferences + "/plan";
    public const string LetterInstruction = "Send correspondence through the records desk.";
    public const string TelephoneInstruction = "Call Helen directly.";
    public const string ShortNotice = "Call before arrival.";
    public const string PreparationNotice = "Reserve thirty minutes for preparation.";
    public const string ConsentGiven = "Staff may arrange an interpreter.";
    public const string ConsentWithheld = "Discuss support directly with Helen.";
    public const string Book = "Book the spoken-language interpreter.";
    public const string PreparePack = "Prepare the translated admission pack.";
    public const string Standard = "Use the standard admission guide.";

    public static readonly AppEvent Save = AppEvent.Declare();

    public static IReadOnlyList<ContactChoice<string>> Channels { get; } = [new("Telephone", "telephone"), new("Letter", "letter")];

    public static IReadOnlyList<ContactChoice<int>> Notice { get; } = [new("Thirty minutes", 30), new("No advance notice", 0)];

    public static IReadOnlyList<ContactChoice<bool>> Permissions { get; } = [new("Consent given", true), new("Consent withheld", false)];

    public static IReadOnlyList<CommunicationLanguage> Languages { get; } =
    [
        new("English", "en", "Latin script", new(new(false, "Admissions"), new(false))),
        new("Punjabi", "pa", "Gurmukhi script", new(new(true, "Punjabi scheduling"), new(true))),
        new("Spanish", "es", "Latin script", new(new(false, "Admissions"), new(true))),
    ];

    public string? Channel { get; init; } = "telephone";

    public int? LeadMinutes { get; init; } = 30;

    public bool? Consent { get; init; } = true;

    public CommunicationLanguage Selected { get; init; } = Languages[0];

    public string PreviousLanguage { get; init; } = "English";

    public bool Saved { get; init; }

    public string Guidance => Selected.Support.Spoken.RequiresInterpreter && Consent == true && (Channel == "telephone" || LeadMinutes >= 30)
        ? Book : Selected.Support.Written.NeedsTranslatedPack ? PreparePack : Standard;
}

public sealed record ContactChoice<T>(string Label, T Code);

public sealed record CommunicationLanguage(string Label, string Code, string Script, LanguageSupport Support)
{
    internal string OptionLabel => $"{Label} · {Script}";
}

public sealed record LanguageSupport(SpokenSupport Spoken, WrittenSupport Written);

public sealed record SpokenSupport(bool RequiresInterpreter, string BookingTeam);

public sealed record WrittenSupport(bool NeedsTranslatedPack);

public sealed record CommunicationPlan(string? Channel, int? LeadMinutes, bool? Consent, string LanguageCode, string? PreviousLanguage, string? SelectedLabel,
    string? BookingTeam, bool RequiresInterpreter, bool NeedsTranslatedPack);
