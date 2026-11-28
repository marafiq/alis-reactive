using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CarePlanUploadController : Controller
{
    private const string PlainText = "text/plain";
    private const string NoFile = "no file";

    [HttpGet(CarePages.CarePlanUpload)]
    public IActionResult OfAResident() => View("/Care/AdmissionClinical/CarePlanUpload.cshtml", new CarePlanDocument());

    [HttpPost(CarePlanUploadEndpoints.CarePlans)]
    public IActionResult Save([FromForm] CarePlanDocument document) => Content(document.CarePlan?.FileName ?? NoFile, PlainText);
}

public sealed class CarePlanDocument
{
    public IFormFile? CarePlan { get; set; }
}

public static class CarePlanUploadEndpoints
{
    public const string CarePlans = CarePages.CarePlanUpload + "/care-plans";
}

public static class CarePlanUploads
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
