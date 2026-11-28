using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VisitAdmitController : Controller
{
    private const string PlainText = "text/plain";
    private const string Page = "/Care/Admission/VisitAdmit.cshtml";
    private const string Room = nameof(Admission.Room);

    [HttpGet(CarePages.VisitAdmit)]
    public IActionResult BesideAVisit() => View(Page, new Admission("", VisitAdmitEndpoints.VisitSection));

    [HttpGet(CarePages.VisitAdmitOwnSection)]
    public IActionResult BesideItsOwnSection() => View(Page, new Admission("", VisitAdmitEndpoints.AdmissionSection));

    [HttpGet(VisitAdmitEndpoints.VisitSection)]
    public IActionResult VisitSection() => PartialView("/Care/Admission/VisitSection.cshtml", new FamilyVisit("Ada's daughter"));

    [HttpGet(VisitAdmitEndpoints.AdmissionSection)]
    public IActionResult AdmissionSection() => PartialView("/Care/Admission/AdmissionSection.cshtml", new Admission("", VisitAdmitEndpoints.AdmissionSection));

    [HttpPost(VisitAdmitEndpoints.Admissions)]
    public IActionResult Receive() => Content($"{Room} {Request.Form[Room]}".TrimEnd(), PlainText);
}

public sealed record Admission(string Room, string SectionUrl);

public sealed record FamilyVisit(string Visitor);

public static class VisitAdmitEndpoints
{
    public const string Admissions = CarePages.Area + "admission/visit-admit/admissions";
    public const string VisitSection = CarePages.Area + "admission/visit-admit/visit-section";
    public const string AdmissionSection = CarePages.Area + "admission/visit-admit/admission-section";
}

public static class VisitAdmits
{
    public static readonly AppEvent LoadSection = AppEvent.Declare();

    public static readonly AppEvent Admit = AppEvent.Declare();
}
