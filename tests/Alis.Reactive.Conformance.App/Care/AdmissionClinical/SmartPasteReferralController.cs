using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SmartPasteReferralController : Controller
{
    private const string ReferralNote = "Resident Rose Alvarez, room 12";
    private const string FieldsView = "/Care/AdmissionClinical/SmartPasteReferralFields.cshtml";

    [HttpGet(CarePages.SmartPasteReferral)]
    public IActionResult Referral() => Intake(startsWithoutForm: false);

    [HttpGet(CarePages.SmartPasteReferralSlot)]
    public IActionResult ReferralSlot() => Intake(startsWithoutForm: true);

    [HttpGet(CarePages.SmartPasteReferralFields)]
    public IActionResult Fields() => PartialView(FieldsView, new SmartPasteReferralFields());

    [HttpPost(CarePages.SmartPasteAssist)]
    [ValidateAntiForgeryToken]
    public IActionResult Assist([FromBody] SmartPasteChat? chat)
    {
        var referral = chat?.Messages.FirstOrDefault(message => message.Role == "user")?.Content.Trim();
        var asksForGivenName = chat?.Messages.Any(message => message.Role == "system" && message.Content.Contains("FIELD GivenName^^^", StringComparison.Ordinal)) == true;
        var givenName = asksForGivenName && referral == "USER_DATA: " + ReferralNote ? "Rose" : "NO_DATA";
        return Content("FIELD GivenName^^^" + givenName, "text/plain");
    }

    private IActionResult Intake(bool startsWithoutForm) => View("/Care/AdmissionClinical/SmartPasteReferral.cshtml",
        new SmartPasteReferral(ReferralNote, startsWithoutForm, new SmartPasteReferralFields()));
}

public sealed record SmartPasteReferral(string ReferralNote, bool StartsWithoutForm, SmartPasteReferralFields Fields);

public sealed class SmartPasteReferralFields
{
    public string GivenName { get; init; } = "";
}

public sealed record SmartPasteChat(IReadOnlyList<SmartPasteMessage> Messages);

public sealed record SmartPasteMessage(string Role, string Content);

public static class SmartPasteReferralButtons
{
    public static readonly AppEvent LoadIntake = AppEvent.Declare();
}
