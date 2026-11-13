using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class PhotoConsentController : Controller
{
    [HttpGet(CarePages.PhotoConsentGiven)]
    public IActionResult Given() => View("/Care/AdmissionClinical/PhotoConsent.cshtml", new AdmissionConsent { PhotoConsent = true });

    [HttpGet(CarePages.PhotoConsentWithheld)]
    public IActionResult Withheld() => View("/Care/AdmissionClinical/PhotoConsent.cshtml", new AdmissionConsent { PhotoConsent = false });
}

public sealed class AdmissionConsent
{
    public bool PhotoConsent { get; set; }
}
