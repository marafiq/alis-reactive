using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PhotoDecisionController : Controller
{
    private PhotoDecisionRegister Register => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(PhotoDecisionRegister.Open);

    [HttpGet(CarePages.PhotoDecision)]
    public IActionResult Decision(bool? granted = null, bool pending = false, bool locked = false)
        => View("/Care/AdmissionClinical/PhotoDecision.cshtml", new PhotoDecision(granted ?? Register.Permitted, pending, locked));

    [HttpPost(CarePages.PhotoDecisionSave)]
    public IActionResult Save([FromForm] PhotoDecisionFiling filing)
    {
        if (filing.PhotoConsent is not { } permitted) return BadRequest();

        Register.Record(permitted);
        return Content(Register.Permitted ? "Photo permission recorded" : "Photo refusal recorded", "text/plain");
    }

    [HttpGet(CarePages.PhotoConsentSheet)]
    public IActionResult Sheet(bool plain = false, bool largePrint = false, bool arabic = false, bool houseCaption = false, bool fieldLabelOnly = false)
        => View("/Care/AdmissionClinical/PhotoConsentSheet.cshtml", new PhotoConsentSheet(false, plain, largePrint, arabic, houseCaption, fieldLabelOnly));

    [HttpGet(CarePages.PhotoReleaseFiling)]
    public IActionResult Filing(bool clinical = false, bool localNotice = false)
        => View("/Care/AdmissionClinical/PhotoReleaseFiling.cshtml", new PhotoReleaseFiling(true, clinical, localNotice));

    [HttpPost(CarePages.PhotoReleaseFiling)]
    public async Task<IActionResult> FileRelease()
    {
        var form = await Request.ReadFormAsync();
        var receipt = form.TryGetValue(nameof(PhotoReleaseFiling.PhotoConsent), out var code)
            ? code.ToString() switch
            {
                "clinical" => "Release filed in the clinical record",
                "directory" => "Release filed in the resident directory",
                "" => "Release filed under general consent",
                _ => "Unknown photo filing code"
            }
            : "No photo release filed";
        return View("/Care/AdmissionClinical/PhotoReleaseReceipt.cshtml", new PhotoReleaseReceipt(receipt));
    }

    [HttpGet(CarePages.PhotoWorksheet)]
    public IActionResult Worksheet(bool signed = false, bool reopen = false)
        => View("/Care/AdmissionClinical/PhotoConsentWorksheet.cshtml", new PhotoConsentWorksheet(signed, reopen));
}

public sealed record PhotoDecision(bool PhotoConsent, bool Pending, bool Locked)
{
    public string Signature { get; init; } = string.Empty;
}

public sealed record PhotoDecisionFiling(bool? PhotoConsent);

public sealed record PhotoConsentSheet(bool PhotoConsent, bool Plain, bool LargePrint, bool Arabic, bool HouseCaption, bool FieldLabelOnly)
{
    public const string PrintCaption = "<span class='consent-caption'><style>.consent-caption .house-signature{display:inline}.consent-caption .standard-permission{display:none}</style><span class='house-signature'>Signature required</span><span class='standard-permission'>Photo release</span></span>";

    public string InitialCaption => this switch
    {
        { FieldLabelOnly: true } => string.Empty,
        { HouseCaption: true } => PrintCaption,
        { Arabic: true } => "أوافق على استخدام صورتي للهوية",
        { Plain: true } => "May we use your photo?",
        _ => "I consent to a photo ID"
    };
}

public sealed record PhotoReleaseFiling(bool PhotoConsent, bool Clinical, bool LocalNotice);

public sealed record PhotoReleaseReceipt(string Filing);

public sealed record PhotoConsentWorksheet(bool PhotoConsent, bool Reopen);

internal sealed class PhotoDecisionRegister : IOwnedByAWorld
{
    private volatile bool _permitted;

    internal static PhotoDecisionRegister Open() => new();

    internal bool Permitted => _permitted;

    internal void Record(bool permitted) => _permitted = permitted;

    void IOwnedByAWorld.Take(Seed seed) { }
}
