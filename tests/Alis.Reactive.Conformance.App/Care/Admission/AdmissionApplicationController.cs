using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class AdmissionApplicationController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.AdmissionApplication)]
    public IActionResult ForAPlace() => View("/Care/Admission/AdmissionApplication.cshtml", new AdmissionApplication());

    [HttpPost(AdmissionApplicationEndpoints.Applications)]
    public IActionResult Apply([FromHeader(Name = AdmissionApplicationEndpoints.ApplicantHeader)] string? applicant)
        => Content($"{AdmissionApplicationEndpoints.ApplicantHeader} {applicant}", PlainText);
}

public sealed class AdmissionApplication
{
    public string? Name { get; set; }
}

public static class AdmissionApplicationEndpoints
{
    public const string Applications = CarePages.AdmissionApplication + "/applications";

    public const string ApplicantHeader = "X-Applicant";
}

public static class AdmissionApplications
{
    public static readonly AppEvent Apply = AppEvent.Declare();
}
