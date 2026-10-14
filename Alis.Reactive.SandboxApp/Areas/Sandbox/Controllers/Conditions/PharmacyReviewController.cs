using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.PharmacyReview;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Riverside Pharmacy reviews new medication orders for Maple Grove's second floor. Helen Park takes
/// warfarin, Arthur Bell lisinopril and Walter Cho digoxin; Rosa Diaz is allergic to penicillin. The
/// pharmacy refuses an allergy (409) and a controlled substance without a signed prescription (422).
/// Walter Cho's physician's pager is down, so a page about him fails (503).
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Conditions/PharmacyReview")]
public class PharmacyReviewController : Controller
{
    [HttpGet("")]
    public IActionResult Index() =>
        View("~/Areas/Sandbox/Views/Conditions/PharmacyReview/Index.cshtml", new PharmacyReviewModel());

    [HttpGet("Residents/{residentId}/Profile")]
    public IActionResult Profile(string residentId) => residentId switch
    {
        "helen" => Ok(new PharmacyProfile("Takes warfarin 5 mg daily. No known allergies.")),
        "arthur" => Ok(new PharmacyProfile("Takes lisinopril 10 mg daily. No known allergies.")),
        "rosa" => Ok(new PharmacyProfile("Takes no regular medications. Allergic to penicillin.")),
        "walter" => Ok(new PharmacyProfile("Takes digoxin 0.125 mg daily. No known allergies.")),
        _ => NotFound(),
    };

    [HttpGet("Formulary")]
    public IActionResult Formulary([FromQuery] FormularyLookup lookup) => Normalized(lookup.Medication) switch
    {
        "furosemide" or "lasix" => Ok(new FormularyEntry("furosemide", "20 mg tablet")),
        "aspirin" => Ok(new FormularyEntry("aspirin", "81 mg chewable tablet")),
        "potassium chloride" => Ok(new FormularyEntry("potassium chloride", "10 mEq extended-release tablet")),
        "enalapril" => Ok(new FormularyEntry("enalapril", "5 mg tablet")),
        "amoxicillin" => Ok(new FormularyEntry("amoxicillin", "500 mg capsule")),
        "oxycodone" => Ok(new FormularyEntry("oxycodone", "5 mg tablet")),
        "clarithromycin" => Ok(new FormularyEntry("clarithromycin", "500 mg tablet")),
        _ => NotFound(),
    };

    [HttpPost("Residents/{residentId}/Review")]
    public IActionResult Review(string residentId, [FromBody] PharmacyReviewOrder? order) =>
        (residentId, Normalized(order?.Medication)) switch
        {
            ("helen", "aspirin") => Ok(new PharmacyReviewAnswer("interaction", 7,
                "Major interaction (7 of 10): aspirin with warfarin raises the risk of bleeding.")),
            ("arthur", "potassium chloride") => Ok(new PharmacyReviewAnswer("interaction", 6,
                "Moderate interaction (6 of 10): potassium with lisinopril can raise potassium; check levels in three days.")),
            ("arthur", "enalapril") => Ok(new PharmacyReviewAnswer("duplicate", 8,
                "Arthur Bell already takes lisinopril, another ACE inhibitor.")),
            ("walter", "clarithromycin") => Ok(new PharmacyReviewAnswer("interaction", 9,
                "Major interaction (9 of 10): clarithromycin raises digoxin to toxic levels.")),
            ("rosa", "amoxicillin") => Conflict(new PharmacyRefusal("allergy",
                "Rosa Diaz is allergic to penicillin (hives, 2019); amoxicillin is a penicillin.")),
            (_, "oxycodone") => UnprocessableEntity(new PharmacyRefusal("prescription",
                "Oxycodone is a controlled substance: the pharmacy needs the physician's signed prescription.")),
            _ => ResidentName(residentId) is { } name
                ? Ok(new PharmacyReviewAnswer("clear", 0, $"No interactions with {name}'s current medications."))
                : NotFound(),
        };

    [HttpPost("PagePhysician")]
    public IActionResult PagePhysician([FromBody] PhysicianPageRequest page)
    {
        if (page.ResidentId == "walter")
            return StatusCode(StatusCodes.Status503ServiceUnavailable);

        return ResidentName(page.ResidentId) is { } name && Physician(page.ResidentId) is { } physician
            ? Ok(new PhysicianPaged($"{physician} was paged about {name}: {page.Reason}"))
            : NotFound();
    }

    private static string Normalized(string? medication) => (medication ?? "").Trim().ToLowerInvariant();

    // No static lookup table: a static collection in the sandbox is shared by every browser world.
    private static string? ResidentName(string resident) => resident switch
    {
        "helen" => "Helen Park",
        "arthur" => "Arthur Bell",
        "rosa" => "Rosa Diaz",
        "walter" => "Walter Cho",
        _ => null,
    };

    private static string? Physician(string resident) => resident switch
    {
        "helen" => "Dr. Okafor",
        "arthur" => "Dr. Mensah",
        "rosa" => "Dr. Alvarez",
        _ => null,
    };
}
